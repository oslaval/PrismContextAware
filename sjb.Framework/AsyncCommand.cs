using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

/// The starting point was Nito.Mvvm.AsyncCommand -- but the class here is quite different. 
/// Objectives relative to Nito.Mvvm.AsyncCommand: 
/// 1. Works "out of the box" -- no need to initialize the weak reference handler in the base class.
/// 2. Works with multiple signatures for the execute function.
/// 3. By default have CanExecute = false when executing, but also able to override this behavior.
/// 4. For a cancellable command, automatically pass in a cancellation token and have a cancel command tied to it. 

namespace sjb.Framework
{
    /// <summary>
    /// An async version of <see cref="ICommand"/>.
    /// </summary>
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);

        Nito.Mvvm.NotifyTask Execution { get; }

        bool IsExecuting { get; }

        ProgressReport ProgressReport { get; }

        ICommand CancelCommand { get; }
    }

    public class ProgressReport : Progress<object>
    {
    }

    public abstract class NotifyBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> propertyValues;

        protected NotifyBase()
        {
            propertyValues = new Dictionary<string, object>();
        }

        protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                propertyValues[propertyName] = value;                
            }
            else
            {
                propertyValues.Add(propertyName, value);
            }
            OnPropertyChanged(propertyName);
        }

        protected void SetValue<T>(T value, Expression<Func<T>> propertyExpression)
        {
            MemberExpression? body = propertyExpression.Body as MemberExpression;
            ConstantExpression? expression = body.Expression as ConstantExpression;

            if (propertyValues.ContainsKey(body.Member.Name))
            {
                propertyValues[body.Member.Name] = value;
            }
            else
            {
                propertyValues.Add(body.Member.Name, value);
            }
            OnPropertyChanged(body.Member.Name);
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                return (T)propertyValues[propertyName];
            }
            return default(T);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            MemberExpression? body = propertyExpression.Body as MemberExpression;
            ConstantExpression? expression = body.Expression as ConstantExpression;
            OnPropertyChanged(body.Member.Name);
        }
    }

    /// <summary>
    /// Base class for AsyncCommand, also used to implement CancelCommand.
    /// </summary>
    public abstract class WeakCommandBase : NotifyBase, ICommand
    {
        /// <summary>
        /// The local implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { WeakCollectionOfEventHandler.Add(value); }
            remove { WeakCollectionOfEventHandler.Remove(value); }
        }

        /// <summary>
        /// The weak collection of delegates for <see cref="CanExecuteChanged"/>. This is used instead of a normal
        /// event in order to avoid a memory leak on some platforms, that occurs due to buttons not unregistering
        /// themselves from the event. 
        /// </summary>
        private Nito.Mvvm.WeakCollection<EventHandler> WeakCollectionOfEventHandler
        {
            get { return _weakCollectionOfEventHandler ?? (_weakCollectionOfEventHandler = new Nito.Mvvm.WeakCollection<EventHandler>()); }
        }
        private Nito.Mvvm.WeakCollection<EventHandler> _weakCollectionOfEventHandler;

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>; in other words, this.CanExecuteChanged?.Invoke( this, EventArgs.Empty); 
        /// </summary>
        protected void OnCanExecuteChanged()
        {
            if (_weakCollectionOfEventHandler != null)
            {
                foreach (EventHandler? eventHandler in _weakCollectionOfEventHandler.GetLiveItems())
                {
                    eventHandler(this, EventArgs.Empty);
                }
            }
        }

        public abstract void Execute(object parameter = null);

        public abstract bool CanExecute(object parameter = null);
    }


    /// <summary>
    /// A basic asynchronous command, which (by default) is disabled while the command is executing.
    /// </summary>
    public sealed class AsyncCommand : WeakCommandBase, IAsyncCommand
    {
        #region CanExecuteWhileExecuting

        /// <summary>
        /// Indicates whether the command can execute while it is already executing. 
        /// The default is false. 
        /// An exception occurs if you try to set CanExecuteWhileExecuting = true while an exclusion group is present. 
        /// </summary>
        public bool CanExecuteWhileExecuting
        {
            get
            {
                return _canExecuteWhileExecuting;
            }
            set
            {
                if (value == CanExecuteWhileExecuting) { return; }
                if (value == true && ExclusionGroup != null)
                {
                    throw new InvalidOperationException("Cannot execute while executing and also be in an exclusion group.");
                }
                _canExecuteWhileExecuting = value;
                OnPropertyChanged(() => CanExecuteWhileExecuting);
                OnCanExecuteChanged();
            }
        }
        private bool _canExecuteWhileExecuting = false;

        #endregion

        #region AreMutuallyExclusive, AreNotMutuallyExclusive, ExclusionGroup

        /// <summary>
        /// Use this function to specify that a set of commands are mutually exclusive, i.e. that when 
        /// one of them CanExecute then the other ones cannot execute (CanExecute == false).
        /// The mutually exclusive commands are in an "exclusion group".
        /// An exception occurs if CanExecuteWhileExecuting = true for any command in the list. 
        /// </summary>
        /// <param name="commands"></param>
        public static void AreMutuallyExclusive(IEnumerable<AsyncCommand> commands)
        {
            foreach (AsyncCommand? cmd in commands)
            {
                if (cmd.CanExecuteWhileExecuting)
                {
                    throw new InvalidOperationException("Cannot be in an exclusion group and also execute while executing.");
                }
                cmd.ExclusionGroup = commands;
            }
            foreach (AsyncCommand? cmd in commands)
            {
                cmd.OnCanExecuteChanged();
            }
        }
        /// <summary>
        /// Sets each command's exclusion group to null. 
        /// This typically reverses the effect of AreMutuallyExclusive.
        /// </summary>
        /// <param name="commands"></param>
        public static void AreNotMutuallyExclusive(IEnumerable<AsyncCommand> commands)
        {
            foreach (AsyncCommand? cmd in commands)
            {
                cmd.ExclusionGroup = null;
            }
            foreach (AsyncCommand? cmd in commands)
            {
                cmd.OnCanExecuteChanged();
            }
        }
        private IEnumerable<AsyncCommand> ExclusionGroup { get; set; }

        #endregion

        #region CanExecute

        /// <summary>
        /// This is an infrastructure method intended to be used by the framework only. 
        /// Implements ICommand.CanExecute for the AsyncCommand. 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override bool CanExecute(object parameter = null)
        {
            if (IsExecuting && !CanExecuteWhileExecuting)
            { // can't execute because it is already executing
                return false;
            }
            if (ExclusionGroup != null)
            {
                foreach (AsyncCommand? cmd in ExclusionGroup)
                {
                    if (cmd.IsExecuting) { return false; } // can't execute because one of the other excluding commands is executing (possibly this one)
                }
            }
            return CanExecuteOnlyIfProperty == null ? true : CanExecuteOnlyIfProperty.Value; // test for user restriction if there is one.
        }

        /// <summary>
        /// Creates a binding to a boolean property that must be true for the command to be enabled.
        /// Use this method to restrict execution of the command so that it cannot always be executed. 
        /// Use of this method is optional. 
        /// Two other conditions also need to be satisfied in order for the command to be enabled: 
        /// (1) if CanExecuteWhileExecuting = false, then the command must not already be executing; and (2) if a list 
        /// of excluding commands has been set by a call to AreMutuallyExclusive, then it must be that none of them
        /// is already executing. 
        /// </summary>
        public void CanExecuteOnlyIf<TSource>(TSource source, Expression<Func<TSource, bool>> propertyPath)
        {
            CanExecuteOnlyIfProperty = BoundFunction.Create(source, (b) => b, propertyPath);
            CanExecuteOnlyIfProperty.PropertyChanged += (s, e) => { base.OnCanExecuteChanged(); };
            base.OnCanExecuteChanged();
        }
        public void CanExecuteOnlyIf(BoundFunction<bool> boundFunction)
        {
            CanExecuteOnlyIfProperty = boundFunction;
            CanExecuteOnlyIfProperty.PropertyChanged += (s, e) => { base.OnCanExecuteChanged(); };
            base.OnCanExecuteChanged();
        }
        private BoundFunction<bool> CanExecuteOnlyIfProperty { get; set; }

        #endregion

        #region Execution, IsExecuting, Execute, ExecuteAsync

        /// <summary>
        /// Represents the execution of the synchronous or asynchronous command.
        /// </summary>
        public Nito.Mvvm.NotifyTask Execution { get; private set; }

        /// <summary>
        /// Indicates whether the command is currently executing or not.
        /// </summary>
        public bool IsExecuting
        {
            get
            {
                return (Execution != null && Execution.IsNotCompleted);
            }
        }

        /// <summary>
        /// Implements ICommand.Execute
        /// </summary>
        /// <param name="parameter"></param>
        public override async void Execute(object parameter = null)
        {
            await ExecuteAsync(parameter);
        }

        /// <summary>
        /// Executes the synchronous or asynchronous command asynchronously. If the command delegate accepts
        /// a ProgressReport then all exceptions are placed on the Task returned by ExecuteAsync (and then
        /// are rethrown by the await in Execute). If the command delegate does not accept a ProgressReport
        /// then UserInformationExceptions are retained inside this.Execution while other excptions are
        /// put into the Task returned by ExecuteAsync; these latter will be rethrown from Execute. 
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public async Task ExecuteAsync(object parameter)
        {
            _CancelCommand = NeedsCancellationToken ? new CancelAsyncCommand(this) : null;
            OnPropertyChanged("CancelCommand");

            ProgressReport = NeedsProgressReport ? new ProgressReport() : null;
            OnPropertyChanged("ProgressReport");

            //var rethrow = (this.ProgressReport != null) ? NotifyTaskExceptions.RethrowAll : NotifyTaskExceptions.RethrowAllButUserInformationExceptions; 

            Execution = Nito.Mvvm.NotifyTask.Create(() => ExecuteDelegateAsync(parameter));

            OnCanExecuteChanged();
            _CancelCommand?._OnCanExecuteChanged();
            OnPropertyChanged("Execution");

            OnPropertyChanged("IsExecuting");
            if (ExclusionGroup != null)
            {
                foreach (AsyncCommand? cmd in ExclusionGroup)
                {
                    cmd.OnCanExecuteChanged();
                }
            }

            await Execution.TaskCompleted;

            OnCanExecuteChanged();
            _CancelCommand?._OnCanExecuteChanged();

            OnPropertyChanged("IsExecuting");
            if (ExclusionGroup != null)
            {
                foreach (AsyncCommand? cmd in ExclusionGroup)
                {
                    cmd.OnCanExecuteChanged();
                }
            }
        }

        #endregion

        #region primary constructor, ExecuteDelegateAsync, CanExecuteDelegate, CanCancelDelegate, NeedsCancellationToken, NeedsProgressReport

        private AsyncCommand(
            Func<object, CancellationToken, ProgressReport, Task> execute, // "object" is the command parameter
            bool needsCancellationToken,
            bool needsProgressReport
        )
        {
            _execute = execute;
            NeedsCancellationToken = needsCancellationToken;
            NeedsProgressReport = needsProgressReport;
            base.OnCanExecuteChanged();
        }

        /// <summary>
        /// If RunOnDifferentThread is true then the async command will be executed on a different 
        /// thread pool thread i.e. await Task.Run(execute(...)). This would be appropriate, for example, 
        /// if your execute delegate is a synchronous computation-bound function. If RunOnDifferentThread
        /// is false then the async command will be executed in a background Task on the current thread, 
        /// i.e. await execute(...). This would be appropriate, for example, for an execute delegate that
        /// is an asynch I/O bound function.
        /// </summary>
        public bool RunOnDifferentThread
        {
            get { return _runOnDifferentThread; }
            set { base.SetValue(value, () => RunOnDifferentThread); }
        }

        private readonly bool _runOnDifferentThread = false;

        /// <summary>
        /// Invokes the _execute delegate asynchronously, capturing any exceptions in the returned Task. 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private async Task ExecuteDelegateAsync(object parameter)
        {
            CancellationToken cancellationToken;
            if (NeedsCancellationToken)
            {
                cancellationToken = _CancelCommand.Token;
            }
            else
            {
                cancellationToken = new CancellationToken(); // dummy token
            }
            // Since we are awaiting immediataly, the ExecuteDelegateAsync method returns immediately.
            if (RunOnDifferentThread)
            {
                await Task.Run(() => _execute(parameter, cancellationToken, ProgressReport), cancellationToken);
            }
            else
            {
                await _execute(parameter, cancellationToken, ProgressReport);
            }
        }
        private readonly Func<object, CancellationToken, ProgressReport, Task> _execute; // must have one of the types shown in the constructors

        /// <summary>
        /// Indicates whether a CancellationToken is needed
        /// </summary>
        private bool NeedsCancellationToken { get; }

        /// <summary>
        /// Indicates whether a ProgressReport is needed. 
        /// </summary>
        private bool NeedsProgressReport { get; }

        #endregion

        #region alternative constructors for various execute signatures

        /// <summary>
        /// Creates a new asynchronous command, with the specified delegate as its implementation.
        /// </summary>
        /// <param name="execute">The function used to implement
        /// <see cref="ICommand.Execute(object)"/> and <see cref="ISyncAsyncCommand.ExecuteAsync(object)"/>.
        /// The delegate or lambda expression passed in may be either asynchronous or synchronous; in either case
        /// it will be executed asynchronously. 
        /// The delegate or lambda expression you pass in also may take any combination (or none) of
        /// the following arguments, in any order: 
        /// an argument of type "object", if present, is the parameter to the command;
        /// an argument of type "CancellationToken", if present, is a cancellation token that can be signalled 
        /// using this.CancelCommand; 
        /// an argument of type ProgressReport, if present, is an argument used to report progress back to the UI. 
        /// Whichever of these arguments your delegate accepts, will be provided by the AsycCommand when executing
        /// the command asynchronously. If the delegate accepts a ProgressReport then it should report user error 
        /// messages by setting the ErrorMessage property of the ProgressReport. In this case, all exceptions will 
        /// be propagated back to the thread which invoked Execute or which awaited ExecuteAsync (typically the UI thread). 
        /// If the delegate does not accept a ProgressReport then it should report user error messages by throwing
        /// a UserInformationException, which will be retained in this.Execution (i.e. in this.Execution.ErrorMessage, 
        /// this.Execution.Exception, etc). In this case, all exceptions that are not UserInformationExceptions (i.e.
        /// exceptions representing bugs) will be propagated back to the thread which invoked Execute or which 
        /// awaited ExecuteAsync.
        /// </param>
        public AsyncCommand(Func<Task> execute) : this((o, c, p) => execute(), false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<CancellationToken, Task> execute) : this((o, c, p) => execute(c), true, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<object, Task> execute) : this((o, c, p) => execute(o), false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<ProgressReport, Task> execute) : this((o, c, p) => execute(p), false, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<ProgressReport, CancellationToken, Task> execute) : this((o, c, p) => execute(p, c), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<CancellationToken, ProgressReport, Task> execute) : this((o, c, p) => execute(c, p), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<object, CancellationToken, ProgressReport, Task> execute) : this((o, c, p) => execute(o, c, p), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<CancellationToken, object, ProgressReport, Task> execute) : this((o, c, p) => execute(c, o, p), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<CancellationToken, ProgressReport, object, Task> execute) : this((o, c, p) => execute(c, p, o), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<ProgressReport, CancellationToken, object, Task> execute) : this((o, c, p) => execute(p, c, o), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<ProgressReport, object, CancellationToken, Task> execute) : this((o, c, p) => execute(p, o, c), true, true)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Func<object, ProgressReport, CancellationToken, Task> execute) : this((o, c, p) => execute(o, p, c), true, true)
        { }


        /// <summary>
        /// Creates a new asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action execute) : this((o, c, p) => { execute(); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<CancellationToken> execute) : this((o, c, p) => { execute(c); return Task.CompletedTask; }, true, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<object> execute) : this((o, c, p) => { execute(o); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<ProgressReport> execute) : this((o, c, p) => { execute(p); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<ProgressReport, CancellationToken> execute) : this((o, c, p) => { execute(p, c); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<CancellationToken, ProgressReport> execute) : this((o, c, p) => { execute(c, p); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<object, CancellationToken, ProgressReport> execute) : this((o, c, p) => { execute(o, c, p); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<CancellationToken, object, ProgressReport> execute) : this((o, c, p) => { execute(c, o, p); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<CancellationToken, ProgressReport, object> execute) : this((o, c, p) => { execute(c, p, o); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<ProgressReport, CancellationToken, object> execute) : this((o, c, p) => { execute(p, c, o); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<ProgressReport, object, CancellationToken> execute) : this((o, c, p) => { execute(p, o, c); return Task.CompletedTask; }, false, false)
        { }

        /// <summary>
        /// Creates a new synchronous or asynchronous command, with the specified delegates as its implementation.
        /// </summary>
        public AsyncCommand(Action<object, ProgressReport, CancellationToken> execute) : this((o, c, p) => { execute(o, p, c); return Task.CompletedTask; }, false, false)
        { }


        #endregion

        #region CancelCommand, ProgressReport

        public ProgressReport ProgressReport { get; private set; }

        private CancelAsyncCommand _CancelCommand { get; set; }

        public ICommand CancelCommand => _CancelCommand;

        private sealed class CancelAsyncCommand : WeakCommandBase
        {
            public CancelAsyncCommand(AsyncCommand parent)
            {
                Parent = parent;
                Debug.Assert(Parent.NeedsCancellationToken);
            }

            private readonly AsyncCommand Parent;

            private readonly CancellationTokenSource CTS = new CancellationTokenSource();

            public CancellationToken Token => CTS.Token;

            public override void Execute(object parameter = null)
            {
                CTS.Cancel();
                OnCanExecuteChanged();
            }

            /// <summary>
            /// This is an infrastructure method intended to be used by the framework only. 
            /// Implements ICommand.CanExecute for the CancelCommand (if any) associated with an AsyncCommand. 
            /// </summary>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public override bool CanExecute(object parameter = null)
            {
                if (!Parent.IsExecuting)
                { // can't cancel because there is no execution in progress to be cancelled.
                    return false;
                }
                if (Token.IsCancellationRequested)
                { // can't cancel because there a cancellation has already been requested.
                    return false;
                }
                return CanExecuteOnlyIfProperty == null ? true : CanExecuteOnlyIfProperty.Value; // test for user restriction if there is one.
            }

            public void CanExecuteOnlyIf<TSource>(TSource source, Expression<Func<TSource, bool>> propertyPath)
            {
                CanExecuteOnlyIfProperty = BoundFunction.Create(source, (b) => b, propertyPath);
                CanExecuteOnlyIfProperty.PropertyChanged += (s, e) => { base.OnCanExecuteChanged(); };
                base.OnCanExecuteChanged();
            }
            public void CanExecuteOnlyIf(BoundFunction<bool> boundFunction)
            {
                CanExecuteOnlyIfProperty = boundFunction;
                CanExecuteOnlyIfProperty.PropertyChanged += (s, e) => { base.OnCanExecuteChanged(); };
                base.OnCanExecuteChanged();
            }
            private BoundFunction<bool> CanExecuteOnlyIfProperty { get; set; }

            public void _OnCanExecuteChanged()
            {
                base.OnCanExecuteChanged();
            }
        }

        #endregion
    }
}