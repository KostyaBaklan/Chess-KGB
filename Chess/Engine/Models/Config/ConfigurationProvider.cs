using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class ConfigurationProvider: IConfigurationProvider
    {
        public ConfigurationProvider(IAlgorithmConfiguration algorithmConfiguration, IEvaluationProvider evaluation)
        {
            AlgorithmConfiguration = algorithmConfiguration;
            Evaluation = evaluation;
        }

        #region Implementation of IConfigurationProvider

        public IAlgorithmConfiguration AlgorithmConfiguration { get; }
        public IEvaluationProvider Evaluation { get; }

        #endregion
    }
}