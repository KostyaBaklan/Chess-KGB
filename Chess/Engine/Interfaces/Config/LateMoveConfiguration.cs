namespace Engine.Interfaces.Config
{
    public class LateMoveConfiguration
    {
        public int LmrDepthThreshold { get; set; }
        public int[] LmrLateDepthThreshold { get; set; }
        public int LmrDepthReduction { get; set; }
        public int LmrSubSearchDepth { get; set; }
        public bool UseSubSearch { get; set; }
    }
}