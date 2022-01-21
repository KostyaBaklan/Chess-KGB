using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int GetValue(int piece);
        int GetValue(int piece, int square, Phase phase);
        int GetFullValue(int piece, int square, Phase phase);
        int GetPawnValue(int factor = 1);
        int GetPenaltyValue(int factor = 1);
        int GetUnitValue(int factor = 1);
        int GetMateValue(bool isWhite);
        int Evaluate(IPosition position);
        void Initialize(short depth);
    }
}