namespace Engine.Interfaces.Config
{
    public interface IGeneralConfiguration
    {
        bool UseEvaluationCache { get; }
        int GameDepth { get; }
    }
}