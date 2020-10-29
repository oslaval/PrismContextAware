using PrismContextAware.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace PrismContextAware.Services
{
    /// <summary>
    /// View aware service that provides the following
    /// 1. Events for ViewLoaded / ViewUnloaded (WPF and SL)
    /// 2. Events for ViewActivated / ViewDeactivated (WPF Only)
    /// 3. Views current Dispatcher
    /// 4. If the view implements <c>IViewCreationContextProvider</c>
    ///    the current Views Context will also be available to allow
    ///    the ViewModel to obtain some view specific contextual information
    /// </summary>
    public class ViewAwareStatusService : IViewAwareStatus
    {

        #region Data
        private WeakReference _weakViewInstance;
        #endregion

        #region IViewAwareStatus Members

        private readonly IList<WeakAction> _loadedHandlers = new List<WeakAction>();
        public event Action ViewLoaded
        {
            add
            {
                _loadedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _unloadedHandlers = new List<WeakAction>();
        public event Action ViewUnloaded
        {
            add
            {
                _unloadedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _activatedHandlers = new List<WeakAction>();
        public event Action ViewActivated
        {
            add
            {
                _activatedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _deactivatedHandlers = new List<WeakAction>();
        public event Action ViewDeactivated
        {
            add
            {
                _deactivatedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        public Dispatcher ViewsDispatcher { get; private set; }

        public object View { get { return _weakViewInstance.Target; } }

        #endregion

        #region IContextAware Members

        public void InjectContext(object view)
        {
            if (_weakViewInstance != null)
            {
                if (_weakViewInstance.Target == view)
                {
                    return;
                }
            }

            // unregister before hooking new events
            if (_weakViewInstance != null && this._weakViewInstance.Target != null)
            {
                object targ = _weakViewInstance.Target;

                if (targ != null)
                {
                    ((FrameworkElement)targ).Loaded -= OnViewLoaded;
                    ((FrameworkElement)targ).Unloaded -= OnViewUnloaded;

                    if (targ is Window w)
                    {
                        w.Activated -= OnViewActivated;
                        w.Deactivated -= OnViewDeactivated;
                    }
                }

            }

            if (view is FrameworkElement x)
            {
                x.Loaded += OnViewLoaded;
                x.Unloaded += OnViewUnloaded;

                if (x is Window w)
                {
                    w.Activated += OnViewActivated;
                    w.Deactivated += OnViewDeactivated;
                }

                //get the Views Dispatcher
                ViewsDispatcher = x.Dispatcher;
                _weakViewInstance = new WeakReference(x);

            }
        }

        #endregion

        #region Private Helpers

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            foreach (WeakAction loadedHandler in _loadedHandlers)
            {
                loadedHandler.GetMethod().DynamicInvoke();
            }
        }

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (WeakAction unloadedHandler in _unloadedHandlers)
            {
                unloadedHandler.GetMethod().DynamicInvoke();
            }
        }

        private void OnViewActivated(object sender, EventArgs e)
        {
            foreach (WeakAction activatedHandler in _activatedHandlers)
            {
                activatedHandler.GetMethod().DynamicInvoke();
            }

        }

        private void OnViewDeactivated(object sender, EventArgs e)
        {
            foreach (WeakAction deactivatedHandler in _deactivatedHandlers)
            {
                deactivatedHandler.GetMethod().DynamicInvoke();
            }
        }

        #endregion
    }
}