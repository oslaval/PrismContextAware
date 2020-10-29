using System;
using System.Windows.Threading;

namespace PrismContextAware.Services.Contracts
{
    public interface IViewAwareStatus : IContextAware
    {
        event Action ViewLoaded;
        event Action ViewUnloaded;

#if !SILVERLIGHT

        event Action ViewActivated;
        event Action ViewDeactivated;
#else
        void PerformCleanUp();
#endif

        Dispatcher ViewsDispatcher { get; }
        Object View { get; }
    }
}
