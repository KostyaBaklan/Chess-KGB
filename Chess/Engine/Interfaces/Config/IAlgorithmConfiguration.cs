namespace Engine.Interfaces.Config
{
    public interface IAlgorithmConfiguration
    {
        int DepthOffset { get; }
        int DepthReduction { get; }
        int MaxEndGameDepth { get; }
        IterativeDeepingConfiguration IterativeDeepingConfiguration { get; }
        AspirationConfiguration AspirationConfiguration { get; }
        NullConfiguration NullConfiguration { get; }
        MultiCutConfiguration MultiCutConfiguration { get; }
        LateMoveConfiguration LateMoveConfiguration { get; }
        PvsConfiguration PvsConfiguration { get; }
        SortingConfiguration SortingConfiguration { get; }
    }
}