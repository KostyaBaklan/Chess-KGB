using System.Collections.Generic;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace OpeningTool
{
    internal class OpeningResult : IResult
    {
        public OpeningResult()
        {
            Value = int.MinValue;
            GameResult = GameResult.Continue;
            Moves = new List<MoveBase>();
        }

        #region Implementation of IResult

        public int Value { get; set; }
        public GameResult GameResult { get; set; }
        public MoveBase Move { get; set; }
        public List<MoveBase> Moves { get; }

        #endregion
    }
}