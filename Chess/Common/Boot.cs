using System;
using System.IO;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Config;
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

            var x = File.ReadAllText(@"Config\StaticTables.json");
            var collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorAdapter);
            container.RegisterInstance<IServiceLocator>(serviceLocatorAdapter);
            container.RegisterInstance<IServiceProvider>(serviceLocatorAdapter);

            var evaluation = configuration.Evaluation;
            IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration,
                new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
                configuration.GeneralConfiguration);
            container.RegisterInstance(configurationProvider);

            IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
            container.RegisterInstance(staticValueProvider);

            container.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
            container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
            container.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
            container.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
            container.RegisterSingleton(typeof(ICheckService), typeof(CheckService));
        }
    }
}