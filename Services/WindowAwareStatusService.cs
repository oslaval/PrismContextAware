using PrismContextAware.Services.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace PrismContextAware.Services
{
    /// <summary>
    /// Window aware service that provides the following Events. Where we are specifically
    /// targetting a Window type. As such this is only available for WPF
    ///    Loaded / Unloaded
    ///    Activated / Deactivated
    ///    WindowClosed / WindowContentRendered / 
    ///    WindowLocationChanged / WindowStateChanged
    /// Windows current Dispatcher
    /// </summary>
    public class WindowAwareStatusService : ViewAwareStatusService, IWindowAwareStatus
    {
        #region Data
        private WeakReference? _weakInstance;
        #endregion

        #region IWindowAwareStatus Members

        //private readonly IList<WeakAction?> _loadedHandlers = new List<WeakAction?>();
        //public event Action Loaded
        //{
        //    add
        //    {
        //        _loadedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
        //    }
        //    remove
        //    {

        //    }
        //}

        //private readonly IList<WeakAction> _unloadedHandlers = new List<WeakAction>();
        //public event Action Unloaded
        //{
        //    add
        //    {
        //        _unloadedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
        //    }
        //    remove
        //    {

        //    }
        //}

        private readonly IList<WeakAction> _activatedHandlers = new List<WeakAction>();
        public event Action Activated
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
        public event Action Deactivated
        {
            add
            {
                _deactivatedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _closedHandlers = new List<WeakAction>();
        public event Action WindowClosed
        {
            add
            {
                _closedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly WeakEvent<EventHandler<CancelEventArgs>> _viewWindowClosingEvent = new WeakEvent<EventHandler<CancelEventArgs>>();
        public event EventHandler<CancelEventArgs> WindowClosing
        {
            add
            {
                _viewWindowClosingEvent.Add(value);
            }
            remove
            {
                _viewWindowClosingEvent.Remove(value);
            }
        }

        private readonly IList<WeakAction> _contentRenderedHandlers = new List<WeakAction>();
        public event Action WindowContentRendered
        {
            add
            {
                _contentRenderedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _locationChangedHandlers = new List<WeakAction>();
        public event Action WindowLocationChanged
        {
            add
            {
                _locationChangedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        private readonly IList<WeakAction> _stateChangedHandlers = new List<WeakAction>();
        public event Action WindowStateChanged
        {
            add
            {
                _stateChangedHandlers.Add(new WeakAction(value.Target, value.Method, typeof(Action)));
            }
            remove
            {

            }
        }

        //public Dispatcher Dispatcher { get; private set; }

        //public object Context { get { return _weakInstance.Target; } }

        #endregion

        #region IContextAware Members
        
        public override void InjectContext(object context)
        {
            if (_weakInstance != null)
            {
                if (_weakInstance.Target == context)
                {
                    return;
                }
            }

            base.InjectContext(context);

            // unregister before hooking new events
            if (_weakInstance != null && _weakInstance.Target != null)
            {
                object target = _weakInstance.Target;

                if (target != null)
                {
                    if (target is Window targetWindow)
                    {
                        //targetWindow.Loaded -= OnViewLoaded;
                        //targetWindow.Unloaded -= OnViewUnloaded;
                        targetWindow.Activated -= OnViewActivated;
                        targetWindow.Deactivated -= OnViewDeactivated;
                        targetWindow.Closed -= OnViewWindowClosed;
                        targetWindow.Closing -= OnViewWindowClosing;
                        targetWindow.ContentRendered -= OnViewWindowContentRendered;
                        targetWindow.LocationChanged -= OnViewWindowLocationChanged;
                        targetWindow.StateChanged -= OnViewWindowStateChanged;
                    }
                }
            }

            if (context is Window contextWindow)
            {
                //contextWindow.Loaded += OnViewLoaded;
                //contextWindow.Unloaded += OnViewUnloaded;
                contextWindow.Activated += OnViewActivated;
                contextWindow.Deactivated += OnViewDeactivated;
                contextWindow.Closed += OnViewWindowClosed;
                contextWindow.Closing += OnViewWindowClosing;
                contextWindow.ContentRendered += OnViewWindowContentRendered;
                contextWindow.LocationChanged += OnViewWindowLocationChanged;
                contextWindow.StateChanged += OnViewWindowStateChanged;

                //get the Views Dispatcher
                //Dispatcher = contextWindow.Dispatcher;
                _weakInstance = new WeakReference(contextWindow);
            }
        }
        #endregion

        #region Private Helpers

        //private void OnViewLoaded(object sender, RoutedEventArgs e)
        //{
        //    foreach (WeakAction loadedHandler in _loadedHandlers)
        //    {
        //        loadedHandler.GetMethod().DynamicInvoke();
        //    }
        //}

        //private void OnViewUnloaded(object sender, RoutedEventArgs e)
        //{
        //    foreach (WeakAction unloadedHandler in _unloadedHandlers)
        //    {
        //        unloadedHandler.GetMethod().DynamicInvoke();
        //    }
        //}

        private void OnViewActivated(object? sender, EventArgs e)
        {
            foreach (WeakAction activatedHandler in _activatedHandlers)
            {
                activatedHandler?.GetMethod()?.DynamicInvoke();
            }

        }

        private void OnViewDeactivated(object? sender, EventArgs e)
        {
            foreach (WeakAction deactivatedHandler in _deactivatedHandlers)
            {
                deactivatedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        private void OnViewWindowClosed(object? sender, EventArgs e)
        {
            foreach (WeakAction closedHandler in _closedHandlers)
            {
                closedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        private void OnViewWindowClosing(object sender, CancelEventArgs e)
        {
            _viewWindowClosingEvent.Raise(this, e);
        }

        private void OnViewWindowContentRendered(object? sender, EventArgs e)
        {
            foreach (WeakAction contentRenderedHandler in _contentRenderedHandlers)
            {
                contentRenderedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        private void OnViewWindowLocationChanged(object? sender, EventArgs e)
        {
            foreach (WeakAction locationChangedHandler in _locationChangedHandlers)
            {
                locationChangedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        private void OnViewWindowStateChanged(object? sender, EventArgs e)
        {
            foreach (WeakAction stateChangedHandler in _stateChangedHandlers)
            {
                stateChangedHandler?.GetMethod()?.DynamicInvoke();
            }
        }
        #endregion
    }
}