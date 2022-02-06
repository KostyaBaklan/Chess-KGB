﻿namespace Engine.Models.Config
{
    public class BoardEvaluation
    {
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
        public short RentgenValue { get; set; }
        public int RookConnectionValue { get; set; }
    }
}