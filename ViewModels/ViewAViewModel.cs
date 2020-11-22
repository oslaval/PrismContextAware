using PrismContextAware.Services.Contracts;

namespace PrismContextAware.ViewModels
{
    public class ViewAViewModel : IViewAwareStatusInjectionAware
    {
        public void InitialiseViewAwareService(IViewAwareStatus view)
        {
            view.Loaded += View_Loaded;
            view.Unloaded += View_Unloaded;
        }

        private void View_Unloaded()
        {
            
        }

        private void View_Loaded()
        {
            
        }
    }
}
