# PrismContextAware
- using prism 8 with WeakAction/WeakEvent

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                object vm = ContainerLocator.Container.Resolve(type);

                if (vm is IViewAwareStatusInjectionAware contextViewModel)
                {
                    ViewAwareStatusService service = new ViewAwareStatusService();
                    service.InjectContext(view);
                    contextViewModel.InitialiseViewAwareService(service);
                    return vm;
                }

                if (vm is IWindowAwareStatusInjectionAware contextWindowModel)
                {
                    WindowAwareStatusService service = new WindowAwareStatusService();
                    service.InjectContext(view);
                    contextWindowModel.InitialiseViewAwareService(service);
                    return vm;
                }

                return vm;
            });
        }


    public class MainWindowModel : IWindowAwareStatusInjectionAware
    {
        public MainWindowModel()
        {

        }

        public void InitialiseWindowAwareService(IWindowAwareStatus window)
        {
            window.WindowClosing += Window_WindowClosing;
        }

        private void Window_WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = MessageBox.Show("Exit ?", "ContextAware", MessageBoxButton.YesNo) == MessageBoxResult.No;
        }
    }
