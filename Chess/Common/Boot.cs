using System;
using System.IO;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Newtonsoft.Json;
using Unity;

namespace Common
{
    public class Boot
    {
        public static void SetUp()
        {
            IUnityContainer container = new UnityContainer();
            ServiceLocatorAdapter serviceLocatorAdapter = new ServiceLocatorAdapter(container);

            var s = File.ReadAllText(@"Config\Configuration.json");
            var configuration = JsonConvert.DeserializeObject<Configuration>(s);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorAdapter);
            container.RegisterInstance<IServiceLocator>(serviceLocatorAdapter);
            container.RegisterInstance<IServiceProvider>(serviceLocatorAdapter);

            container.RegisterInstance<IConfiguration>(configuration);

            container.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
            container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
            container.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
            container.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
            container.RegisterSingleton(typeof(ICheckService), typeof(CheckService));
        }
    }
}