using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter : IMoveSorter
    {
        protected readonly KillerMoveCollection[] Moves;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected readonly IPosition Position;
        private readonly List<IMove> _moves;

        protected MoveSorter(IPosition position)
        {
            _moves = new List<IMove>(8);
            Position = position;
            Moves = new KillerMoveCollection[256];
            for (var i = 0; i < Moves.Length; i++)
            {
                Moves[i] = new KillerMoveCollection();
            }

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(IMove move)
        {
            int depth = MoveHistoryService.GetPly();
            Moves[depth].Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMoveCollection Order(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, IMove pvNode)
        {
            int depth = MoveHistoryService.GetPly();
            if (depth < 0)
            {
                var collection = new MoveCollection(Comparer);
                foreach (var move in moves)
                {
                    collection.AddNonCapture(move);
                }
                collection.Build();
                return collection;
            }

            return pvNode != null ? OrderInternal(attacks, moves, Moves[depth], pvNode) : OrderInternal(attacks, moves, Moves[depth]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMoveCollection Order(IEnumerable<IAttack> attacks)
        {
            var sortedAttacks = OrderAttacks(attacks);

            AttackCollection collection = new AttackCollection(Comparer);

            OrderAttacks(collection, sortedAttacks);

            collection.Build();
            return collection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Dictionary<Square, DynamicSortedList<IAttack>> OrderAttacks(IEnumerable<IAttack> attacks)
        {
            var board = Position.GetBoard();
            var attackComparer = new AttackComparer();

            Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks = new Dictionary<Square, DynamicSortedList<IAttack>>();
            foreach (var attack in attacks)
            {
                attack.Captured = board.GetPiece(attack.To);
                if (sortedAttacks.TryGetValue(attack.To, out var set))
                {
                    set.Push(attack);
                }
                else
                {
                    sortedAttacks.Add(attack.To, new DynamicSortedList<IAttack>(attackComparer, attack));
                }
            }

            return sortedAttacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection, Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks, IAttack pv)
        {
            _moves.Clear();
            var maxValue = 0;
            int maxIndex = -1;
            var index = 0;
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
                        if (attackValue > maxValue)
                        {
                            maxValue = attackValue;
                            maxIndex = index;
                        }
                        _moves.Add(attack);
                        index++;
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

            AddWinCaptures(collection, maxIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection,
            Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks)
        {
            _moves.Clear();
            var maxValue = 0;
            int maxIndex = -1;
            var index = 0;
            var board = Position.GetBoard();
            foreach (var sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();

                    int attackValue = board.StaticExchange(attack);
                    if (attackValue > 0)
                    {
                        if (attackValue > maxValue)
                        {
                            maxValue = attackValue;
                            maxIndex = index;
                        }
                        _moves.Add(attack);
                        index++;
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

            AddWinCaptures(collection,  maxIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddWinCaptures(AttackCollection collection,int maxIndex)
        {
            if (_moves.Count <= 0) return;
            if (maxIndex != 0)
            {
                var temp = _moves[0];
                _moves[0] = _moves[maxIndex];
                _moves[maxIndex] = temp;
            }

            for (var i = 0; i < _moves.Count; i++)
            {
                collection.AddWinCapture(_moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void DecideTrade(AttackCollection collection, IAttack attack)
        {
            collection.AddTrade(attack);
        }

        protected abstract IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove pvNode);
    }
}