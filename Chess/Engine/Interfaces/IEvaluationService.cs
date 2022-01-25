using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int GetValue(int piece);
        int GetValue(int piece, int square, Phase phase);
        int GetFullValue(int piece, int square, Phase phase);
        int GetMateValue();
        int Evaluate(IPosition position);
        void Initialize(short depth);

        int GetUnitValue();
        int GetPenaltyValue();
        int GetMinorDefendedByPawnValue();

        int GetBlockedPawnValue();
        int GetPassedPawnValue();
        int GetDoubledPawnValue();
        int GetIsolatedPawnValue();
        int GetBackwardPawnValue();

        int GetNotAbleCastleValue();
        int GetEarlyQueenValue();
        int GetDoubleBishopValue();
        int GetRookOnOpenFileValue();
    }
}