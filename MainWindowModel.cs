using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrismContextAware.Services.Contracts;
using PrismContextAware.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PrismContextAware
{
    public class MainWindowModel : BindableBase, IWindowAwareStatusInjectionAware
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; private set; }

        public MainWindowModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (navigatePath != null)
            {
                var region = _regionManager.Regions["ContentRegion"];
                region.RemoveAll();
                region.NavigationService.Journal.Clear();
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
            }
        }

        public void InitialiseWindowAwareService(IWindowAwareStatus window)
        {
            window.Loaded += Window_Loaded;
            window.Activated += Window_Activated;
            window.WindowClosing += Window_WindowClosing; ;
        }

        private void Window_Loaded()
        {
            
        }

        private void Window_Activated()
        {
            
        }

        private void Window_WindowClosing(object? sender, CancelEventArgs e)
        {
            e.Cancel = MessageBox.Show("Exit ?", "ContextAware", MessageBoxButton.YesNo) == MessageBoxResult.No;
        }
    }
}
