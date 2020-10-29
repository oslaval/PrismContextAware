using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace PrismContextAware.Services.Contracts
{
    public interface IWindowAwareStatus : IContextAware
    {
        event Action Loaded;
        event Action Unloaded;
        event Action Activated;
        event Action Deactivated;

        event Action WindowClosed;
        event Action WindowContentRendered;
        event Action WindowLocationChanged;
        event Action WindowStateChanged;
        event EventHandler<CancelEventArgs> WindowClosing;

        Dispatcher Dispatcher { get; }
        object View { get; }
    }
}
