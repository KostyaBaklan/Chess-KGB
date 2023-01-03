using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.AlphaBeta;
using Engine.Strategies.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Strategies.LMR
{
    public abstract partial class LateMoveReductionStrategyBase : AlphaBetaStrategy
    {
        private StrategyBase _subSearchStrategy;

        protected int LmrSubSearchDepth;
        protected bool UseSubSearch;
        protected readonly bool[][] CanReduce;
        protected readonly byte[][] Reduction;

        protected StrategyBase SubSearchStrategy
        {
            get
            {
                return _subSearchStrategy ?? (_subSearchStrategy = CreateSubSearchStrategy());
            }
        }

        protected LateMoveReductionStrategyBase(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();

            LmrSubSearchDepth = configurationProvider
                    .AlgorithmConfiguration.LateMoveConfiguration.LmrSubSearchDepth;
            UseSubSearch = configurationProvider
                    .AlgorithmConfiguration.LateMoveConfiguration.UseSubSearch;

            CanReduce = InitializeReducableTable();
            Reduction = InitializeReductionTable();

            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();

            MoveBase pv = pvMove;
            if (pv == null)
            {
                if (Table.TryGet(Position.GetKey(), out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            var count = moves.Length;
            if (CheckMoves(count, out var res)) return res;

            if (count > 1)
            {
                if (UseSubSearch)
                {
                    moves = SubSearch(moves, alpha, beta, depth);
                }

                int d = depth - 1;
                int b = -beta;

                if (MoveHistory.IsLastMoveNotReducible())
                {
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        var value = -Search(b, -alpha, d);

                        Position.UnMake();
                        if (value > result.Value)
                        {
                            result.Value = value;
                            result.Move = move;
                        }

                        if (value > alpha)
                        {
                            alpha = value;
                        }

                        if (alpha < beta) continue;
                        break;
                    }
                }
                else
                {
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        int value;
                        if (move.CanReduce && !move.IsCheck && CanReduce[depth][i])
                        {
                            value = -Search(b, -alpha, Reduction[depth][i]);
                            if (value > alpha)
                            {
                                value = -Search(b, -alpha, d);
                            }
                        }
                        else
                        {
                            value = -Search(b, -alpha, d);
                        }

                        Position.UnMake();
                        if (value > result.Value)
                        {
                            result.Value = value;
                            result.Move = move;
                        }


                        if (value > alpha)
                        {
                            alpha = value;
                        }

                        if (alpha < beta) continue;
                        break;
                    }
                }
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));
            }

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;
                if (entry.Depth >= depth)
                {
                    if (entry.Value > alpha)
                    {
                        alpha = entry.Value;
                    }
                    if (alpha >= beta)
                        return entry.Value;
                }
                else
                {
                    shouldUpdate = Position.GetPhase() != Phase.End;
                }
                pv = GetPv(entry.PvMove);
            }

            var moves = IsFutility(alpha, depth)
                ? Position.GetAllAttacks(Sorters[depth])
                : Position.GetAllMoves(Sorters[depth], pv);

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            int r;
            int d = depth - 1;
            int b = -beta;
            int value = int.MinValue;

            MoveBase bestMove = null;
            MoveBase move;

            if (MoveHistory.IsLastMoveNotReducible())
            {
                for (var i = 0; i < count; i++)
                {
                    move = moves[i];
                    Position.Make(move);

                    r = -Search(b, -alpha, d);

                    if (r > value)
                    {
                        value = r;
                        bestMove = move;
                    }

                    Position.UnMake();

                    if (value > alpha)
                    {
                        alpha = value;
                    }

                    if (alpha < beta) continue;
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    move = moves[i];
                    Position.Make(move);

                    if (move.CanReduce && !move.IsCheck && CanReduce[depth][i])
                    {
                        r = -Search(b, -alpha, Reduction[depth][i]);
                        if (r > alpha)
                        {
                            r = -Search(b, -alpha, d);
                        }
                    }
                    else
                    {
                        r = -Search(b, -alpha, d);
                    }

                    if (r > value)
                    {
                        value = r;
                        bestMove = move;
                    }

                    Position.UnMake();

                    if (value > alpha)
                    {
                        alpha = value;
                    }

                    if (alpha < beta) continue;
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
            }

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }

        protected abstract byte[][] InitializeReductionTable();
        protected abstract bool[][] InitializeReducableTable();
        protected abstract StrategyBase CreateSubSearchStrategy();

        protected MoveBase[] SubSearch(MoveBase[] moves, int alpha, int beta, int depth)
        {
            ValueMove[] valueMoves = new ValueMove[moves.Length];
            for (var i = 0; i < moves.Length; i++)
            {
                Position.Make(moves[i]);

                valueMoves[i] = new ValueMove { Move = moves[i], Value = -SubSearchStrategy.Search(-beta, -alpha, depth - LmrSubSearchDepth) };

                Position.UnMake();
            }

            Array.Sort(valueMoves);

            for (int i = 0; i < valueMoves.Length; i++)
            {
                moves[i] = valueMoves[i].Move;
            }

            return moves;
        }
    }
}
