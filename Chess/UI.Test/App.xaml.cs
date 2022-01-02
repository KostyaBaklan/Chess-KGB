using System.Windows;
using Algorithms.Services;
using CommonServiceLocator;
using Infrastructure.BootStrap;
using Infrastructure.DataStructures;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Services;
using Unity;
using Unity.Lifetime;

namespace UI.Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Overrides of Application

        protected override void OnStartup(StartupEventArgs e)
        {
            var unityContainer = new UnityContainer();

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(unityContainer));

            var coordinateProvider = new CoordinateProvider();
            unityContainer.RegisterInstance<ICoordinateProvider>(coordinateProvider);

            var moveProvider = new MoveProvider(coordinateProvider);
            unityContainer.RegisterInstance<IMoveProvider>(moveProvider);

            unityContainer.RegisterType<IKillerMoveCollection, KillerMoveCollection>(new TransientLifetimeManager());
            unityContainer.RegisterSingleton<IMoveFormatter, MoveFormatter>();
            unityContainer.RegisterSingleton<IFigureMap, FigureMap>();
            unityContainer.RegisterSingleton<IMoveHistoryService, MoveHistoryService>();
            unityContainer.RegisterSingleton<ICheckService, CheckService>();
            unityContainer.RegisterSingleton<IOpennigService, OpennigService>();
            unityContainer.RegisterSingleton<ILog, Logger>();
            unityContainer.RegisterSingleton<IEvaluationService, EvaluationService>();

            base.OnStartup(e);
        }

        #endregion
    }
}
