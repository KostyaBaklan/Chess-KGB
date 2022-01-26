using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class GeneralConfiguration : IGeneralConfiguration
    {
        #region Implementation of IGeneralConfiguration

        public bool UseEvaluationCache { get; set; }

        #endregion
    }
    public class AlgorithmConfiguration : IAlgorithmConfiguration
    {
        #region Implementation of IAlgorithmConfiguration

        public int InitialDepth { get; set; }
        public int NullWindow { get; set; }
        public int AspirationWindow { get; set; }

        #endregion
    }
}