namespace Engine.Interfaces.Config
{
    public interface IConfigurationProvider
    {
        IAlgorithmConfiguration AlgorithmConfiguration { get; }
        IEvaluationProvider Evaluation { get; }
    }
}
