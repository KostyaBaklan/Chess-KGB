using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class AlgorithmConfiguration : IAlgorithmConfiguration
    {
        #region Implementation of IAlgorithmConfiguration

        public int InitialDepth { get; set; }

        #endregion
    }
}