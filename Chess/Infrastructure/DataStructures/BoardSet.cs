using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Infrastructure.Interfaces.Position;
using Infrastructure.Moves;

namespace Infrastructure.DataStructures
{
    public class BoardSet
    {
        private readonly Coordinate[] _items;
        private readonly int[] _occuped;
        private readonly int[] _index;
        private int _lastOccuped;

        public BoardSet()
        {
            var size = 64;
            _items = new Coordinate[size];
            _occuped = new int[size];
            _index = new int[size];
            var coordinateProvider = ServiceLocator.Current.GetInstance<ICoordinateProvider>();
            for (var i = 0; i < _items.Length; i++)
            {
                _items[i] = coordinateProvider.GetCoordinate(i);
            }
        }

        public int Count { get { return _lastOccuped; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte rank)
        {
            _occuped[_lastOccuped] = rank;
            _index[rank] = _lastOccuped++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte rank)
        {
            var occuped = _index[rank];
            --_lastOccuped;
            if (_lastOccuped <= occuped) return;

            _occuped[occuped] = _occuped[_lastOccuped];
            _index[_occuped[_lastOccuped]] = occuped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Coordinate> Items()
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                yield return _items[_occuped[i]];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> Coordinates()
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                yield return _occuped[i];
            }
        }

        public bool IsOn(int coordinate)
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                if (_occuped[i] == coordinate) return true;
            }

            return false;
        }
    }
}
