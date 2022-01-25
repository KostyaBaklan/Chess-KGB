using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class StaticEvaluation: IStaticEvaluation
    {
        #region Implementation of IStaticEvaluation

        public short Unit { get; set; }
        public short Penalty { get; set; }
        public short Mate { get; set; }
        public short Factor { get; set; }

        public short NotAbleCastleValue { get; set; }
        public short EarlyQueenValue { get; set; }
        public short DoubleBishopValue { get; set; }
        public short MinorDefendedByPawnValue { get; set; }
        public short BlockedPawnValue { get; set; }
        public short PassedPawnValue { get; set; }
        public short DoubledPawnValue { get; set; }
        public short IsolatedPawnValue { get; set; }
        public short BackwardPawnValue { get; set; }
        public short RookOnOpenFileValue { get; set; }

        #endregion
    }
}