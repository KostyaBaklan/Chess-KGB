using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter : IMoveSorter
    {
        protected readonly IKillerMoveCollection[] Moves;
        protected readonly AttackList AttackList;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected IKillerMoveCollection CurrentKillers;
        protected readonly IPosition Position;

        protected AttackCollection AttackCollection;
        protected MoveCollection MoveCollection;
        protected readonly IBoard Board;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        protected MoveSorter(IPosition position, IMoveComparer comparer)
        {
            AttackList = new AttackList();
            Board = position.GetBoard();
            Comparer = comparer;
            Moves = ServiceLocator.Current.GetInstance<IKillerMoveCollectionFactory>().CreateMoves();
            Position = position;

            AttackCollection = new AttackCollection(comparer);
            MoveCollection = new MoveCollection(comparer);

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(short move)
        {
            Moves[MoveHistoryService.GetPly()].Add(move);
        }

        public MoveBase[] Order(AttackList attacks, MoveList moves, MoveBase pvNode)
        {
            int depth = MoveHistoryService.GetPly();

            if (depth > 0 || pvNode!=null)
            {
                CurrentKillers = Moves[depth];
                return pvNode != null
                    ? OrderInternal(attacks, moves, pvNode)
                    : OrderInternal(attacks, moves);
            }

            MoveList moveList = new MoveList();
            foreach (var move in moves)
            {
                moveList.Add(move);
            }

            moveList.FullSort();

            var m = new MoveBase[moveList.Count];
            moveList.CopyTo(m, 0);
            return m;
        }

        public MoveBase[] Order(AttackList attacks)
        {
            if (attacks.Count == 0) return new MoveBase[0];
            if (attacks.Count == 1) return new MoveBase[]{attacks[0]};

            OrderAttacks(AttackCollection, attacks);

            return AttackCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection, AttackList sortedAttacks, short pv)
        {
            AttackList.Clear();
            for (int i = 0; i < sortedAttacks.Count; i++)
            {
                var attack = sortedAttacks[i];
                if (attack.Key == pv)
                {
                    collection.AddHashMove(attack);
                    continue;
                }

                attack.Captured = Board.GetPiece(attack.To);

                int attackValue = Board.StaticExchange(attack);
                if (attackValue > 0)
                {
                    attack.See = attackValue;
                    AttackList.Add(attack);
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

            if (AttackList.Count <= 0) return;
            if (AttackList.Count > 1)
            {
                AttackList.SortBySee();
            }
            collection.AddWinCapture(AttackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OrderAttacks(AttackCollection collection,
            AttackList sortedAttacks)
        {
            AttackList.Clear();
            for (int i = 0; i < sortedAttacks.Count; i++)
            {
                var attack = sortedAttacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                int attackValue = Board.StaticExchange(attack);
                if (attackValue > 0)
                {
                    attack.See = attackValue;
                    AttackList.Add(attack);
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

            if (AttackList.Count <= 0) return;
            if (AttackList.Count > 1)
            {
                AttackList.SortBySee();
            }
            collection.AddWinCapture(AttackList);
        }

        protected abstract MoveBase[] OrderInternal(AttackList attacks, MoveList moves);
        protected abstract MoveBase[] OrderInternal(AttackList attacks, MoveList moves,  MoveBase pvNode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessPromotion(MoveBase move, AttackCollection collection)
        {
            Position.Make(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    MoveProvider.GetBlackAttacksTo(move.To.AsByte(), AttackList);
                    WhiteStaticExchange(move, collection);
                }
                else
                {
                    MoveProvider.GetWhiteAttacksTo(move.To.AsByte(), AttackList);
                    BlackStaticExchange(move, collection);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhiteStaticExchange(MoveBase move, AttackCollection collection)
        {
            if (AttackList.Count == 0)
            {
                collection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = Piece.WhitePawn;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    collection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    collection.AddLooseCapture(move);
                }
                else
                {
                    collection.AddTrade(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackStaticExchange(MoveBase move, AttackCollection collection)
        {
            if (AttackList.Count == 0)
            {
                collection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = Piece.BlackPawn;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    collection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    collection.AddLooseCapture(move);
                }
                else
                {
                    collection.AddTrade(move);
                }
            }
        }
    }
}