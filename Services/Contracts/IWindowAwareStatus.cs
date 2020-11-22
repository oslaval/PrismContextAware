using System;
using System.ComponentModel;

namespace PrismContextAware.Services.Contracts
{
    public interface IWindowAwareStatus : IViewAwareStatus
    {
        event Action Activated;
        event Action Deactivated;
        event Action WindowClosed;
        event Action WindowContentRendered;
        event Action WindowLocationChanged;
        event Action WindowStateChanged;
        event EventHandler<CancelEventArgs> WindowClosing;
    }
}
