using System;
using System.Globalization;
using System.IO;
using System.Windows;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Kgb.ChessApp.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;

namespace Kgb.ChessApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var s = File.ReadAllText(@"Config\Configuration.json");
            var configuration = JsonConvert.DeserializeObject<Configuration>(s);

            containerRegistry.RegisterInstance(typeof(IConfiguration), configuration);

            containerRegistry.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
            containerRegistry.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
            containerRegistry.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
            containerRegistry.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
            containerRegistry.RegisterSingleton(typeof(ICheckService), typeof(CheckService));

            containerRegistry.RegisterSingleton(typeof(StartViewModel));
            containerRegistry.RegisterSingleton(typeof(GameViewModel));
        }

        protected override Window CreateShell()
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            regionManager.RegisterViewWithRegion("Main", typeof(StartView));
            regionManager.RegisterViewWithRegion("Main", typeof(GameView));

            return new Shell();
        }

        #region Overrides of PrismApplicationBase

        protected override void  ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.Assembly.FullName;
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);
                return Type.GetType(viewModelName);
            });

            ViewModelLocationProvider.SetDefaultViewModelFactory(vmType =>
            {
                var resolve = ServiceLocator.Current.GetInstance(vmType);
                return resolve;
            });
        }

        #endregion
    }
}
