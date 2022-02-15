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
        public int NonPvIterations { get; set; }
        public int DepthOffset { get; set; }
        public int DepthReduction { get; set; }
        public int NullDepthReduction { get; set; }
        public int MultiCutReduction { get; set; }
        public int MultiCutDepth { get; set; }
        public int MultiCutRequirement { get; set; }
        public int MultiCutMoves { get; set; }
        public int LmrDepthThreshold { get; set; }
        public int LmrLateDepthThreshold { get; set; }
        public int LmrDepthReduction { get; set; }

        #endregion
    }
}