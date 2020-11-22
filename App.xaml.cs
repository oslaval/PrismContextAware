using DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using PrismContextAware.Services;
using PrismContextAware.Services.Contracts;
using PrismContextAware.Views;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace PrismContextAware
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IViewAwareStatus, ViewAwareStatusService>();
            containerRegistry.Register<IWindowAwareStatus, WindowAwareStatusService>();

            containerRegistry.RegisterForNavigation<ViewA>();
            containerRegistry.RegisterForNavigation<ViewB>();
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                string? viewName = viewType.FullName;                
                viewName = viewName?.Replace(".Views.", ".ViewModels.");
                string? viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;                                              
                string? suffix = viewName!=null && (viewName.EndsWith("View") | viewName.EndsWith("Window")) ? "Model" : "ViewModel";                
                string viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
                return Type.GetType(viewModelName);
            });
            
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                object vm = ContainerLocator.Container.Resolve(type);

                if (vm is IViewAwareStatusInjectionAware contextViewModel)
                {
                    IViewAwareStatus service = ContainerLocator.Container.Resolve<IViewAwareStatus>();
                    service.InjectContext(view);
                    contextViewModel.InitialiseViewAwareService(service);
                    return vm;
                }

                if (vm is IWindowAwareStatusInjectionAware contextWindowModel)
                {
                    IWindowAwareStatus service = ContainerLocator.Container.Resolve<IWindowAwareStatus>();
                    service.InjectContext(view);
                    contextWindowModel.InitialiseWindowAwareService(service);
                    return vm;
                }

                return vm;
            });
        }
    }
}
