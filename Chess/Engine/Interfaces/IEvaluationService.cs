namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int Evaluate(IPosition position);
    }
}