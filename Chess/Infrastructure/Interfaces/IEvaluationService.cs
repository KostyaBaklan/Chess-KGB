using Infrastructure.Interfaces.Position;

namespace Infrastructure.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int Evaluate(IPosition position);
    }
}