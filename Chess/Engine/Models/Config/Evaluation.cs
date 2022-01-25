﻿namespace Engine.Models.Config
{
    public class Evaluation
    {
        #region Implementation of IEvaluation

        public StaticEvaluation Static { get; set; }
        public PieceEvaluation Piece { get; set; }

        #endregion
    }
}