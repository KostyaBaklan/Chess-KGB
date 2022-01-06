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

        protected MoveSorter(IPosition position)
        {
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
        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            //var enumerable = moves.ToArray();

            //var staticSort = enumerable.ToList();
            //staticSort.Sort(new StaticComparer());

            //var differenceSort = enumerable.ToList();
            //differenceSort.Sort(new DifferenceComparer());

            int depth = MoveHistoryService.GetPly();
            if (depth < 0) return moves;

            if (pvNode != null)
            {
                return OrderInternal(moves, Moves[depth], pvNode, cutMove);
            }

            if (cutMove != null)
                return OrderInternal(moves, Moves[depth], cutMove);
            return OrderInternal(moves, Moves[depth]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMoveCollection Order(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, IMove pvNode,
            IMove cutMove)
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

            if (pvNode != null)
            {
                return OrderInternal(attacks,moves, Moves[depth], pvNode, cutMove);
            }

            if (cutMove != null)
                return OrderInternal(attacks, moves, Moves[depth], cutMove);
            return OrderInternal(attacks, moves, Moves[depth]);
        }

        public IMoveCollection Order(IEnumerable<IAttack> attacks)
        {
            var sortedAttacks = OrderAttacks(attacks);

            AttackCollection collection = new AttackCollection(Comparer);

            OrderAttacks(collection, sortedAttacks);

            collection.Build();
            return collection;
        }

        protected Dictionary<Square, DynamicSortedList<IAttack>> OrderAttacks(IEnumerable<IAttack> attacks)
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

        protected void OrderAttacks(MoveCollection collection, Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks, IAttack pv)
        {
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
                        collection.AddWinCapture(attack);
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
        }

        protected void OrderAttacks(AttackCollection collection, Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks)
        {
            var board = Position.GetBoard();
            foreach (var sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();

                    int attackValue = board.StaticExchange(attack);
                    if (attackValue > 0)
                    {
                        collection.AddWinCapture(attack);
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
        }

        protected abstract IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove pvNode);
        protected abstract IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove pvNode, IMove cutMove);

        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove cutMove);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection move, IMove pvNode, IMove cutMove);
    }
}