using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class HistoryHeuristic:IHistoryHeuristic
    {
        private readonly int[,] _whiteHistory;
        private readonly int[,] _blackHistory;

        private readonly IMoveHistoryService _historyService;

        public HistoryHeuristic(IMoveHistoryService historyService)
        {
            _whiteHistory = new int[64,64];
            _blackHistory = new int[64, 64];
            _historyService = historyService;
        }

        #region Implementation of IHistoryHeuristic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(IMove move)
        {
            var ply = _historyService.GetPly();
            ply = ply * ply + move.Difference;
            if (move.Piece.IsWhite())
            {
                _whiteHistory[move.From.AsByte(), move.To.AsByte()] += ply;
            }
            else
            {
                _blackHistory[move.From.AsByte(), move.To.AsByte()] += ply;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(IMove move)
        {
            return move.Piece.IsWhite() ? _whiteHistory[move.From.AsByte(), move.To.AsByte()] : _blackHistory[move.From.AsByte(), move.To.AsByte()];
        }

        #endregion
    }
}
