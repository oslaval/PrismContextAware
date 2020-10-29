# PrismContextAware
- using prism 8

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
