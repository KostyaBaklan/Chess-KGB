using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class ConfigurationProvider: IConfigurationProvider
    {
        public ConfigurationProvider(IAlgorithmConfiguration algorithmConfiguration, IEvaluationProvider evaluation, IGeneralConfiguration generalConfiguration)
        {
            AlgorithmConfiguration = algorithmConfiguration;
            Evaluation = evaluation;
            GeneralConfiguration = generalConfiguration;
        }

        #region Implementation of IConfigurationProvider

        public IGeneralConfiguration GeneralConfiguration { get;  }
        public IAlgorithmConfiguration AlgorithmConfiguration { get; }
        public IEvaluationProvider Evaluation { get; }

        #endregion
    }
}