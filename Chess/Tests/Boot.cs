using System;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Services;
using Unity;

namespace Tests
{
    internal class Boot
    {
        public static void SetUp()
        {
            IUnityContainer container = new UnityContainer();
            ServiceLocatorAdapter serviceLocatorAdapter = new ServiceLocatorAdapter(container);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorAdapter);
            container.RegisterInstance<IServiceLocator>(serviceLocatorAdapter);
            container.RegisterInstance<IServiceProvider>(serviceLocatorAdapter);

            container.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
            container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
            container.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
            container.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
            container.RegisterSingleton(typeof(ICheckService), typeof(CheckService));
            container.RegisterSingleton(typeof(IHistoryHeuristic), typeof(HistoryHeuristic));
        }
    }
}