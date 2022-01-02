using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Services
{
    public class EvaluationService: IEvaluationService
    {
        //private readonly ZobristDictionary<int> _table;
        private readonly Dictionary<ulong,int> _table;

        public EvaluationService()
        {
            //_table = new ZobristDictionary<int>(20000213);
            _table = new Dictionary<ulong, int>(20000213);
        }

        #region Implementation of ICacheService

        public int Size => _table.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _table.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate(IPosition position)
        {
            if (_table.TryGetValue(position.GetKey(), out var value))
            {
                return value;
            }

            if (_table.Count > 20000000)
            {
                _table.Clear();
            }

            value = position.GetValue();
            _table.Add(position.GetKey(), value);
            return value;
        }

        #endregion
    }
}
