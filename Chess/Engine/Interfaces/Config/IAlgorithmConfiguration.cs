namespace Engine.Interfaces.Config
{
    public interface IAlgorithmConfiguration
    {
        int NonPvIterations { get; }
        int DepthOffset { get; }
        int DepthReduction { get; }
        IterativeDeepingConfiguration IterativeDeepingConfiguration { get; }
        AspirationConfiguration AspirationConfiguration { get; }
        NullConfiguration NullConfiguration { get; }
        MultiCutConfiguration MultiCutConfiguration { get; }
        LateMoveConfiguration LateMoveConfiguration { get; }
    }
}