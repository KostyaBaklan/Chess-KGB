using Algorithms.DataStructures;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Services
{
    public class CheckService: ICheckService
    {
        private readonly IMoveProvider _moveProvider;
        private readonly BooleanTable _whiteTable;
        private readonly BooleanTable _blackTable;

        public CheckService(IMoveProvider moveProvider)
        {
            _moveProvider = moveProvider;
            _whiteTable = new BooleanTable(30094277);
            _blackTable = new BooleanTable(30094277);
        }

        #region Implementation of ICheckService

        public int Size => _whiteTable.Count + _blackTable.Count;

        public void Clear()
        {
            _whiteTable.Clear();
            _blackTable.Clear();
        }

        public bool IsBlackCheck(ulong key, IBoard board)
        {
            if (_blackTable.TryGet(key, out var isCheck)) return isCheck;

            Clear(_blackTable);

            isCheck = _moveProvider.AnyBlackCheck(board);
            _blackTable.Add(key, isCheck);
            return isCheck;
        }

        private static void Clear(BooleanTable table)
        {
            if (table.Count >= 30000000)
            {
                table.Clear();
            }
        }

        public bool IsWhiteCheck(ulong key, IBoard board)
        {
            if (_whiteTable.TryGet(key, out var isCheck)) return isCheck;

            Clear(_whiteTable);

            isCheck = _moveProvider.AnyWhiteCheck(board);
            _whiteTable.Add(key, isCheck);
            return isCheck;
        }

        #endregion
    }
}
