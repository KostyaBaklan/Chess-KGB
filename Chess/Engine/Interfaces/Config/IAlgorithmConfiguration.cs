namespace Engine.Interfaces.Config
{
    public interface IGeneralConfiguration
    {
        bool UseEvaluationCache { get; }
    }
    public interface IAlgorithmConfiguration
    {
        int InitialDepth { get; }
        int NullWindow { get; }
        int AspirationWindow { get; }
    }
}