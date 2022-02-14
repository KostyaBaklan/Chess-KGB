using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IEvaluationService : ICacheService
    {
        int GetValue(int piece, Phase phase);
        int GetValue(int piece, int square, Phase phase);
        int GetFullValue(int piece, int square, Phase phase);
        int GetMateValue();
        int Evaluate(IPosition position);
        void Initialize(short depth);

        //int GetUnitValue();
        int GetMinorDefendedByPawnValue(Phase phase);

        int GetBlockedPawnValue(Phase phase);
        int GetPassedPawnValue(Phase phase);
        int GetDoubledPawnValue(Phase phase);
        int GetIsolatedPawnValue(Phase phase);
        int GetBackwardPawnValue(Phase phase);

        int GetNotAbleCastleValue(Phase phase);
        int GetEarlyQueenValue(Phase phase);
        int GetDoubleBishopValue(Phase phase);
        int GetRookOnOpenFileValue(Phase phase);
        int GetRentgenValue(Phase phase);
        int GetRookConnectionValue(Phase phase);
        int GetRookOnHalfOpenFileValue(Phase phase);
    }
}