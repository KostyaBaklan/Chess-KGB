namespace Engine.Interfaces.Config
{
    public interface IStaticEvaluation
    {
        short Unit { get; }
        short Penalty { get; }
        short Mate { get; }
        short Factor { get; }
        short NotAbleCastleValue { get; }
        short EarlyQueenValue { get; }
        short DoubleBishopValue { get; }
        short MinorDefendedByPawnValue { get; }
        short BlockedPawnValue { get; }
        short PassedPawnValue { get; }
        short DoubledPawnValue { get; }
        short IsolatedPawnValue { get; }
        short BackwardPawnValue { get; }
        short RookOnOpenFileValue { get; }
    }
}