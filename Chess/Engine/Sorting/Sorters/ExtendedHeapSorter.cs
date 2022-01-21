using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class ExtendedHeapSorter : MoveSorter
    {
        private int _heapSize = 16;
        private readonly MaxHeap _winHeap;

        public ExtendedHeapSorter(IPosition position, IMoveComparer comparer) : base(position)
        {
            Comparer = comparer;
            _winHeap = new MaxHeap(_heapSize, Comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            MoveHeap collection = new MoveHeap(Comparer);

            OrderAttacks(collection, sortedAttacks);

            foreach (var move in moves)
            {
                if (killerMoveCollection.Contains(move) || move.IsPromotion())
                {
                    collection.AddKillerMove(move);
                }
                else
                {
                    collection.AddNonCapture(move);
                }
            }

            collection.Build();
            return collection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            MoveHeap collection = new MoveHeap(Comparer);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(collection, sortedAttacks, attack);
            }
            else
            {
                OrderAttacks(collection, sortedAttacks);
            }

            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    collection.AddHashMove(move);
                }
                else
                {
                    if (killerMoveCollection.Contains(move) || move.IsPromotion())
                    {
                        collection.AddKillerMove(move);
                    }
                    else
                    {
                        collection.AddNonCapture(move);
                    }
                }
            }

            collection.Build();
            return collection;
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OrderAttacks(AttackCollection collection,
            Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks)
        {
            _winHeap.Clear();
            var board = Position.GetBoard();
            foreach (var sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();

                    int attackValue = board.StaticExchange(attack);
                    if (attackValue > 0)
                    {
                        _winHeap.Insert(attack);
                    }
                    else if (attackValue < 0)
                    {
                        collection.AddLooseCapture(attack);
                    }
                    else
                    {
                        DecideTrade(collection, attack);
                    }
                }
            }

            AddWinners(collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OrderAttacks(AttackCollection collection, Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks, IAttack pv)
        {
            _winHeap.Clear();
            var board = Position.GetBoard();
            foreach (var sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();
                    if (attack.Equals(pv))
                    {
                        collection.AddHashMove(attack);
                        continue;
                    }

                    int attackValue = board.StaticExchange(attack);
                    if (attackValue > 0)
                    {
                        _winHeap.Insert(attack);
                    }
                    else if (attackValue < 0)
                    {
                        collection.AddLooseCapture(attack);
                    }
                    else
                    {
                        collection.AddTrade(attack);
                    }
                }
            }

            AddWinners(collection);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddWinners(AttackCollection collection)
        {
            var count = _winHeap.GetCount();
            if (count <= 0) return;

            for (var index = 0; index < count; index++)
            {
                collection.AddWinCapture(_winHeap[index]);
            }
        }
    }
}