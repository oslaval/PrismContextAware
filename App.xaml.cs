using Prism.Ioc;
using Prism.Mvvm;
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

        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                string viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                string viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                string suffix = viewName.EndsWith("View") | viewName.EndsWith("Window") ? "Model" : "ViewModel";
                string viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
                return Type.GetType(viewModelName);
            });

            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                object vm = ContainerLocator.Container.Resolve(type);
                if (vm is IContextAware contextViewModel)
                {
                    contextViewModel.InjectContext(view);
                }
                return vm;
            });
        }
    }
}
