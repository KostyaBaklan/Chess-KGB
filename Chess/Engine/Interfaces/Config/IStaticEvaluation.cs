using Engine.Models.Config;

namespace Engine.Interfaces.Config
{
    public interface IStaticEvaluation
    {
        short Unit { get; }
        short Penalty { get; }
        short Mate { get; }
        short Factor { get; }
        int ThreefoldRepetitionValue { get; }

        BoardEvaluation Opening { get; set; }
        BoardEvaluation Middle { get; set; }
        BoardEvaluation End { get; set; }

        BoardEvaluation GetBoard(byte phase);
    }
}