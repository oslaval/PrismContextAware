using PrismContextAware.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace PrismContextAware.Services
{
    /// <summary>
    /// View aware service that provides the following Events.
    /// - Loaded / Unloaded    
    /// Views current Dispatcher
    /// </summary>
    public class ViewAwareStatusService : IViewAwareStatus
    {
        #region Data
        private WeakReference? _weakViewInstance;
        #endregion

        #region IViewAwareStatus Members

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

        public Dispatcher? Dispatcher { get; private set; }

        public object? Context { get { return _weakViewInstance?.Target; } }

        #endregion

        #region IContextAware Members

        public virtual void InjectContext(object context)
        {
            if (_weakViewInstance != null)
            {
                if (_weakViewInstance.Target == context)
                {
                    return;
                }
            }

            // unregister before hooking new events
            if (_weakViewInstance != null && _weakViewInstance.Target != null)
            {
                object target = _weakViewInstance.Target;

                if (target != null)
                {
                    if (target is FrameworkElement targetElement)
                    {
                        targetElement.Loaded -= OnViewLoaded;
                        targetElement.Unloaded -= OnViewUnloaded;
                    }
                }
            }

            if (context is FrameworkElement contextElement)
            {
                contextElement.Loaded += OnViewLoaded;
                contextElement.Unloaded += OnViewUnloaded;
                
                Dispatcher = contextElement.Dispatcher;
                _weakViewInstance = new WeakReference(contextElement);
            }
        }

        #endregion

        #region Private Helpers

        private void OnViewLoaded(object? sender, RoutedEventArgs e)
        {
            foreach (WeakAction loadedHandler in _loadedHandlers)
            {
                loadedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        private void OnViewUnloaded(object? sender, RoutedEventArgs e)
        {
            foreach (WeakAction unloadedHandler in _unloadedHandlers)
            {
                unloadedHandler?.GetMethod()?.DynamicInvoke();
            }
        }

        #endregion
    }
}