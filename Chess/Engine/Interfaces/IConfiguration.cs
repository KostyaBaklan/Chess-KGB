namespace Engine.Interfaces
{
    public interface IConfiguration
    {
        bool UseEvaluationCache { get; }
        int InitialDepth { get; }
    }
}
