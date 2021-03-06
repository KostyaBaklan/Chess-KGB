namespace Engine.Interfaces.Config
{
    public interface IConfigurationProvider
    {
        IGeneralConfiguration GeneralConfiguration { get; }
        IAlgorithmConfiguration AlgorithmConfiguration { get; }
        IEvaluationProvider Evaluation { get; }
    }
}
