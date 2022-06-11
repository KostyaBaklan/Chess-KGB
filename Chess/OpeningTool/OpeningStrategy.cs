using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.LateMove.Base;

namespace OpeningTool
{
    internal class OpeningStrategy : LmrStrategyBase
    {
        private readonly MoveFilter _filter;
        protected OpeningSorter OpeningSorter;

        public OpeningStrategy(short depth, IPosition position) : base(depth,position)
        {
            _filter = new MoveFilter(MoveProvider);
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
            OpeningSorter = new OpeningSorter(position, new HistoryComparer());
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            OpeningResult result = new OpeningResult();

            MoveBase pv = pvMove;
            var key = Position.GetKey();
            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (pv == null)
            {
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    if ((entry.Depth - depth) % 2 == 0)
                    {
                        pv = MoveProvider.Get(entry.PvMove);
                    }
                }
            }

            var moves = Position.GetAllMoves(OpeningSorter, pv);

            if (Check(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                var isCheck = MoveHistory.GetLastMove().IsCheck;
                if (isCheck)
                {
                    for (var i = 0; i < moves.Length; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        var value = -Search(-beta, -alpha, depth - 1);

                        Position.UnMake();
                        if (value > result.Value)
                        {
                            result.Value = value;
                            result.Moves.Clear();
                            result.Moves.Add(move);
                        }
                        else if (value == result.Value)
                        {
                            result.Moves.Add(move);
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
                    for (var i = 0; i < moves.Length; i++)
                    {
                        var move = moves[i];
                        if (_filter.IsBad(move.Key.ToString()))
                        {
                            continue;
                        }
                        Position.Make(move);

                        int value;
                        if (alpha > -SearchValue && IsLmr(i) && move.CanReduce && !move.IsCheck)
                        {
                            value = -Search(-beta, -alpha, depth - DepthReduction);
                            if (value > alpha)
                            {
                                value = -Search(-beta, -alpha, depth - 1);
                            }
                        }
                        else
                        {
                            value = -Search(-beta, -alpha, depth - 1);
                        }

                        Position.UnMake();

                        if (value > result.Value)
                        {
                            result.Value = value;
                            result.Moves.Clear();
                            result.Moves.Add(move);
                        }
                        else if (value == result.Value)
                        {
                            result.Moves.Add(move);
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
                result.Moves.Clear();
                result.Moves.Add(moves[0]);
            }

            return result;
        }

        private bool Check(MoveBase[] moves, out OpeningResult result)
        {
            result = new OpeningResult();
            if (moves.Length == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck ? GameResult.Mate : GameResult.Pat;
                return true;
            }

            if (!MoveHistory.IsThreefoldRepetition(Position.GetKey())) return false;

            if (Position.GetValue() > 0) return false;

            result.GameResult = GameResult.ThreefoldRepetition;
            return true;
        }
    }
}