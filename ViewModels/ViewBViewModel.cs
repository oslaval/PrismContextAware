using System.Diagnostics;

namespace PrismContextAware.ViewModels
{
    public class ViewBViewModel : ViewAwareStatusViewModel
    {
        protected override void OnLoaded()
        {
            Debug.WriteLine($"OnLoaded ViewB");
        }
        protected override void OnUnloaded()
        {
            Debug.WriteLine($"OnUnloaded ViewB");
        }
    }
}
