namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int GetValue(int piece);
        int GetValue(int piece, int square);
        int GetFullValue(int piece, int square);
        int GetPawnValue(int factor = 1);
        int GetPenaltyValue(int factor = 1);
        int GetUnitValue(int factor = 1);
        int Evaluate(IPosition position);
    }
}