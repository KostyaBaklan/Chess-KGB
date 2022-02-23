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
        protected readonly IKillerMoveCollection[] Moves;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected readonly IPosition Position;

        protected AttackCollection AttackCollection;
        protected MoveCollection MoveCollection;
        protected readonly IBoard Board;

        protected MoveSorter(IPosition position, IMoveComparer comparer)
        {
            Board = position.GetBoard();
            _moves = new MoveList();
            Comparer = comparer;
            Position = position;
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var killerMoveCollectionFactory = ServiceLocator.Current.GetInstance < IKillerMoveCollectionFactory>();
            Moves = new IKillerMoveCollection[configurationProvider
                .GeneralConfiguration.GameDepth];
            for (var i = 0; i < Moves.Length; i++)
            {
                Moves[i] = killerMoveCollectionFactory.Create();
            }

            AttackCollection = new AttackCollection(comparer);
            MoveCollection = new MoveCollection(comparer);

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(IMove move)
        {
            Moves[MoveHistoryService.GetPly()].Add(move.Key);
        }

        public IMove[] Order(AttackList attacks, MoveList moves, IMove pvNode)
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
            moveList.CopyTo(m, 0);
            return m;
        }

        public IMove[] Order(AttackList attacks)
        {
            var sortedAttacks = OrderAttacks(attacks);
            if (sortedAttacks.Count == 0) return new IMove[0];

            OrderAttacks(AttackCollection, sortedAttacks);

            return AttackCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Dictionary<Square, DynamicSortedList<IAttack>> OrderAttacks(AttackList attacks)
        {
            var board = Position.GetBoard();
            var attackComparer = new AttackComparer();

            Dictionary<Square, DynamicSortedList<IAttack>> sortedAttacks = new Dictionary<Square, DynamicSortedList<IAttack>>();
            for (var index = 0; index < attacks.Count; index++)
            {
                var attack = attacks[index];
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
            foreach (DynamicSortedList<IAttack> sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();
                    if (attack.Equals(pv))
                    {
                        collection.AddHashMove(attack);
                        continue;
                    }

                    int attackValue = Board.StaticExchange(attack);
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
            foreach (var sortedAttack in sortedAttacks.Values)
            {
                while (sortedAttack.Count > 0)
                {
                    var attack = sortedAttack.Pop();

                    int attackValue = Board.StaticExchange(attack);
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

        protected abstract IMove[] OrderInternal(AttackList attacks, MoveList moves,
            IKillerMoveCollection killerMoveCollection);
        protected abstract IMove[] OrderInternal(AttackList attacks, MoveList moves,
            IKillerMoveCollection killerMoveCollection, IMove pvNode);
    }
}