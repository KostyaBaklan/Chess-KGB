using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Transposition;

namespace Engine.DataStructures.Hash
{
    public class TranspositionTable : ZobristDictionary<TranspositionEntry>
    {
        private readonly double _blockTimeout;
        private int _nextLevel;
        private readonly int _threshold;
        private bool _isBlocked;

        private readonly ZoobristKeyCollection[] _depthTable;
        private readonly IMoveHistoryService _moveHistory;

        public TranspositionTable(int capacity) : base(capacity)
        {
            _nextLevel = 0;
            _threshold = 2 * capacity / 3;

            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var depth = configurationProvider
                .GeneralConfiguration.GameDepth;
            _blockTimeout = configurationProvider
                .GeneralConfiguration.BlockTimeout;
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();

            _depthTable = new ZoobristKeyCollection[depth];
            for (var i = 0; i < _depthTable.Length; i++)
            {
                _depthTable[i] = new ZoobristKeyCollection();
            }
        }

        #region Overrides of ZobristDictionary<TranspositionEntry>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Replace(ulong key, TranspositionEntry oldItem, TranspositionEntry newItem)
        {
            if (oldItem.Depth < newItem.Depth)
            {
                Table[key] = newItem;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Add(ulong key, TranspositionEntry item)
        {
            Table.Add(key, item);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlocked()
        {
            return _isBlocked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong key, TranspositionEntry item)
        {
            if (_threshold < Table.Count)
            {
                Update();
            }

            while (_isBlocked)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(_blockTimeout));
            }

            Table[key] = item;
            _depthTable[_moveHistory.GetPly()].Add(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            _isBlocked = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var dynamicCollection = _depthTable[_nextLevel];
                    foreach (var key in dynamicCollection.GetAndClear())
                    {
                        Table.Remove(key);
                    }
                }
                finally
                {
                    _nextLevel++;
                    _isBlocked = false;
                }
            });
        }
    }
}