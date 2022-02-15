namespace Engine.Interfaces.Config
{
    public interface IAlgorithmConfiguration
    {
        int InitialDepth { get; }
        int NullWindow { get; }
        int AspirationWindow { get; }
        int AspirationDepth { get; }
        int NonPvIterations { get; }
        int DepthOffset { get; }
        int DepthReduction { get; }
        int NullDepthReduction { get; }
        int MultiCutReduction { get; }
        int MultiCutDepth { get; }
        int MultiCutRequirement { get; }
        int MultiCutMoves { get; }
        int LmrDepthThreshold { get; }
        int LmrLateDepthThreshold { get; }
        int LmrDepthReduction { get; }
    }
}