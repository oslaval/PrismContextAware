using System;
using System.Windows.Threading;

namespace PrismContextAware.Services.Contracts
{
    public interface IViewAwareStatus : IContextAware
    {
        Dispatcher? Dispatcher { get; }

        event Action Loaded;
        event Action Unloaded;
    }
}
