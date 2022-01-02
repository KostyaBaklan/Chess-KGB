using Algorithms.DataStructures;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IntegerTable _table;

        public EvaluationService()
        {
            _table = new IntegerTable(20000213);
        }

        #region Implementation of ICacheService

        public int Size => _table.Count;

        public void Clear()
        {
            _table.Clear();
        }

        public int Evaluate(IPosition position)
        {
            if (_table.TryGet(position.Key, out var value))
            {
                return value;
            }

            if (_table.Count > 20000000)
            {
                _table.Clear();
            }

            value = position.GetValue();
            _table.Add(position.Key, value);
            return value;
        }

        #endregion
    }
}
