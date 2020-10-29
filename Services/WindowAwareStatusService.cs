using PrismContextAware.Services.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PrismContextAware.Services
{
    /// <summary>
    /// Window aware service that provides the following Events. Where we are specifically
    /// targetting a Window type. As such this is only available for WPF
    ///    Loaded / Unloaded
    ///    Activated / Deactivated
    ///    WindowClosed / WindowContentRendered / 
    ///    WindowLocationChanged / WindowStateChanged
    /// 3. Windows current Dispatcher
    /// 4. If the window implements <c>IViewCreationContextProvider</c>
    ///    the current Views Context will also be available to allow
    ///    the ViewModel to obtain some view specific contextual information
    /// </summary>
    public class WindowAwareStatusService : IWindowAwareStatus
    {
        #region Data
        private WeakReference _weakInstance;
        #endregion

        #region IWindowAwareStatus Members

        private readonly IList<WeakAction> _loadedHandlers = new List<WeakAction>();
        public event Action Loaded
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
        public event Action Unloaded
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

        public Dispatcher Dispatcher { get; private set; }

        public object Context { get { return _weakInstance.Target; } }

        #endregion

        #region IContextAware Members

        public void InjectContext(object context)
        {
            if (_weakInstance != null)
            {
                if (_weakInstance.Target == context)
                {
                    return;
                }
            }

            // unregister before hooking new events
            if (_weakInstance != null && _weakInstance.Target != null)
            {
                object targ = _weakInstance.Target;

                if (targ != null)
                {
                    ((FrameworkElement)_weakInstance.Target).Loaded -= OnViewLoaded;
                    ((FrameworkElement)_weakInstance.Target).Unloaded -= OnViewUnloaded;
                    ((Window)_weakInstance.Target).Closed -= OnViewWindowClosed;
                    ((Window)_weakInstance.Target).Closing -= OnViewWindowClosing;
                    ((Window)_weakInstance.Target).ContentRendered -= OnViewWindowContentRendered;
                    ((Window)_weakInstance.Target).LocationChanged -= OnViewWindowLocationChanged;
                    ((Window)_weakInstance.Target).StateChanged -= OnViewWindowStateChanged;

                    if (targ is Window w)
                    {
                        w.Activated -= OnViewActivated;
                        w.Deactivated -= OnViewDeactivated;
                    }
                }

            }

            if (context is FrameworkElement x)
            {
                x.Loaded += OnViewLoaded;
                x.Unloaded += OnViewUnloaded;

                if (x is Window w)
                {
                    w.Activated += OnViewActivated;
                    w.Deactivated += OnViewDeactivated;
                    w.Closed += OnViewWindowClosed;
                    w.Closing += OnViewWindowClosing;
                    w.ContentRendered += OnViewWindowContentRendered;
                    w.LocationChanged += OnViewWindowLocationChanged;
                    w.StateChanged += OnViewWindowStateChanged;
                }

                //get the Views Dispatcher
                Dispatcher = x.Dispatcher;
                _weakInstance = new WeakReference(x);

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

        private void OnViewWindowClosed(object sender, EventArgs e)
        {
            foreach (WeakAction closedHandler in _closedHandlers)
            {
                closedHandler.GetMethod().DynamicInvoke();
            }
        }

        private void OnViewWindowClosing(object sender, CancelEventArgs e)
        {
            _viewWindowClosingEvent.Raise(this, e);
        }

        private void OnViewWindowContentRendered(object sender, EventArgs e)
        {
            foreach (WeakAction contentRenderedHandler in _contentRenderedHandlers)
            {
                contentRenderedHandler.GetMethod().DynamicInvoke();
            }
        }

        private void OnViewWindowLocationChanged(object sender, EventArgs e)
        {
            foreach (WeakAction locationChangedHandler in _locationChangedHandlers)
            {
                locationChangedHandler.GetMethod().DynamicInvoke();
            }
        }

        private void OnViewWindowStateChanged(object sender, EventArgs e)
        {
            foreach (WeakAction stateChangedHandler in _stateChangedHandlers)
            {
                stateChangedHandler.GetMethod().DynamicInvoke();
            }
        }
        #endregion
    }
}