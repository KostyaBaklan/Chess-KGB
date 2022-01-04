using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class SimpleMoveSorter : MoveSorter
    {
        public SimpleMoveSorter(IMoveComparer comparer)
        {
            Comparer = comparer;
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var attackMoves = new List<IMove>(attacks);
            if (attackMoves.Count > 0)
            {
                attackMoves.Sort(Comparer);
                foreach (var move in attackMoves)
                {
                    yield return move;
                }
            }

            var otherMoves = new List<IMove>(64);
            var killer = killerMoveCollection.GetMoves();

            foreach (var move in moves)
            {
                if (killer.Contains(move) || move.IsCastle() || move.IsPromotion())
                {
                    yield return move;
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove cutNode)
        {
            var attackMoves = new List<IMove>(attacks);
            if (attackMoves.Count > 0)
            {
                attackMoves.Sort(Comparer);
                foreach (var move in attackMoves)
                {
                    yield return move;
                }
            }

            var otherMoves = new List<IMove>(64);
            var killer = killerMoveCollection.GetMoves(cutNode);

            foreach (var move in moves)
            {
                if (killer.Contains(move) || move.IsCastle() || move.IsPromotion())
                {
                    yield return move;
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode, IMove cutMove)
        {
            var attackMoves = new List<IMove>();
            foreach (var attack in attacks)
            {
                if (attack.Equals(pvNode))
                {
                    yield return attack;
                }

                attackMoves.Add(attack);
            }

            if (attackMoves.Count > 0)
            {
                attackMoves.Sort(Comparer);
                foreach (var move in attackMoves)
                {
                    yield return move;
                }
            }

            var otherMoves = new List<IMove>(64);
            var killer = killerMoveCollection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    yield return move;
                }

                if (killer.Contains(move) || move.IsCastle() || move.IsPromotion())
                {
                    yield return move;
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection)
        {
            var otherMoves = new List<IMove>(64);
            var killer = collection.GetMoves();

            foreach (var move in moves)
            {
                if (killer.Contains(move))
                {
                    yield return move;
                }
                else if (move.IsAttack() || move.IsCastle() || move.IsPromotion())
                {
                    yield return move;
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection,
            IMove cutMove)
        {
            var otherMoves = new List<IMove>(64);
            var killer = collection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (killer.Contains(move))
                {
                    yield return move;
                }
                else if (move.IsAttack() || move.IsCastle() || move.IsPromotion())
                {
                    yield return move;
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection,
            IMove pvNode, IMove cutMove)
        {
            var otherMoves = new List<IMove>(64);
            var killerMoves = new Queue<IMove>(4);
            var attacks = new Queue<IMove>(12);

            var killer = collection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    yield return move;
                }

                if (killer.Contains(move))
                {
                    killerMoves.Enqueue(move);
                }
                else if (move.IsAttack() || move.IsCastle() || move.IsPromotion())
                {
                    attacks.Enqueue(move);
                }
                else
                {
                    otherMoves.Add(move);
                }
            }

            foreach (var move in killerMoves)
            {
                yield return move;
            }

            foreach (var move in attacks)
            {
                yield return move;
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }
    }
}