using Prism.Mvvm;
using PrismContextAware.Services.Contracts;

namespace PrismContextAware.ViewModels
{
    public class ViewAwareStatusViewModel : BindableBase, IViewAwareStatusInjectionAware
    {
#if DEBUG
        public ViewAwareStatusViewModel()
        {
            System.Diagnostics.Debug.WriteLine($"Created: {GetType().Name} ({GetHashCode()})");
        }

        ~ViewAwareStatusViewModel()
        {
            System.Diagnostics.Debug.WriteLine($"Finalized: {GetType().Name} ({GetHashCode()})");
        }
#endif

        public void InitialiseViewAwareService(IViewAwareStatus view)
        {
            view.Loaded += OnLoaded;
            view.Unloaded += OnUnloaded;
        }

        protected virtual void OnLoaded()
        {
        }

        protected virtual void OnUnloaded()
        {
        }
    }
}
