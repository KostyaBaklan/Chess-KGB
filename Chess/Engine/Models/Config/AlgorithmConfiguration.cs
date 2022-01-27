using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class AlgorithmConfiguration : IAlgorithmConfiguration
    {
        #region Implementation of IAlgorithmConfiguration

        public int InitialDepth { get; set; }
        public int NullWindow { get; set; }
        public int AspirationWindow { get; set; }
        public int AspirationDepth { get; set; }

        #endregion
    }
}