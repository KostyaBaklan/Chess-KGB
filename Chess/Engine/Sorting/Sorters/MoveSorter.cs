using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter : IMoveSorter
    {
        private readonly MoveList _moves;
        protected readonly KillerMoveCollection[] Moves;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected readonly IPosition Position;

        protected AttackCollection AttackCollection;
        protected MoveCollection MoveCollection;

        protected MoveSorter(IPosition position, IMoveComparer comparer)
        {
            _moves = new MoveList();
            Comparer = comparer;
            Position = position;
            Moves = new KillerMoveCollection[ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .GeneralConfiguration.GameDepth];
            for (var i = 0; i < Moves.Length; i++)
            {
                Moves[i] = new KillerMoveCollection();
            }

            AttackCollection = new AttackCollection(comparer);
            MoveCollection = new MoveCollection(comparer);

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(IMove move)
        {
            Moves[MoveHistoryService.GetPly()].Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove[] Order(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, IMove pvNode)
        {
            int depth = MoveHistoryService.GetPly();

            if (depth > 0)
                return pvNode != null
                    ? OrderInternal(attacks, moves, Moves[depth], pvNode)
                    : OrderInternal(attacks, moves, Moves[depth]);

            MoveList moveList = new MoveList();
            foreach (var move in moves)
            {
                moveList.Add(move);
            }

            moveList.Sort(Comparer);

            var m = new IMove[moveList.Count];
            moveList.CopyTo(m,0);
            return m;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove[] Order(IEnumerable<IAttack> attacks)
        {
            var sortedAttacks = OrderAttacks(attacks);
            if(sortedAttacks.Count == 0)return new IMove[0];

            OrderAttacks(AttackCollection, sortedAttacks);

            return AttackCollection.Build();
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
                        collection.AddTrade(attack);
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
                _moves.Swap(0, maxIndex);
            }

            collection.AddWinCapture(_moves);
        }

        protected abstract IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove pvNode);
    }
}