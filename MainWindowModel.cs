using PrismContextAware.Services.Contracts;
using System.ComponentModel;
using System.Windows;

namespace PrismContextAware
{
    public class MainWindowModel : IWindowAwareStatusInjectionAware
    {
        public MainWindowModel()
        {

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
