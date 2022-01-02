using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Infrastructure.Sorters.Comparers
{
    public class MoveThreatComparer : MoveComparerBase
    {
        private readonly Dictionary<IMove, int> _cache;
        private readonly IPosition _position;
        private readonly IMoveProvider _moveProvider;

        public MoveThreatComparer(IPosition position, IMoveProvider moveProvider)
        {
            _cache = new Dictionary<IMove, int>(64);
            _position = position;
            _moveProvider = moveProvider;
        }

        #region Implementation of IComparer<in IMove>

        public override int Compare(IMove x, IMove y)
        {
            if (!_cache.TryGetValue(y, out var yCount))
            {
                yCount = _moveProvider.GetAttacks(y.To, y.Figure, _position.Board).Count();
                _cache[y] = yCount;
            }
            if (!_cache.TryGetValue(x, out var xCount))
            {
                xCount = _moveProvider.GetAttacks(x.To, x.Figure, _position.Board).Count();
                _cache[x] = xCount;
            }

            var t = yCount.CompareTo(xCount);
            if (t != 0) return t;

            var v = y.Value.CompareTo(x.Value);
            return v == 0 ? y.StaticValue.CompareTo(x.StaticValue) : v;
        }

        #endregion

        #region Overrides of MoveComparerBase

        public override void Initialize()
        {
            _cache.Clear();
        }

        #endregion
    }
}
