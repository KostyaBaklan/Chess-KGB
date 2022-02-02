using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.Base
{
    public abstract class StrategyBase : IStrategy
    {
        private bool _isBlocked;
        protected short Depth;
        protected MoveSorter Sorter;
        protected IPosition Position;
        protected int SearchValue;
        protected int ThreefoldRepetitionValue;
        protected int FutilityDepth;
        protected bool UseFutility;
        protected int[][] Margins;

        protected IEvaluationService EvaluationService;
        protected readonly IMoveHistoryService MoveHistory;
        protected readonly IMoveProvider MoveProvider;

        protected StrategyBase(short depth, IPosition position)
        {

            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            SearchValue = configurationProvider
                .Evaluation.Static.Mate;
            ThreefoldRepetitionValue = configurationProvider
                .Evaluation.Static.ThreefoldRepetitionValue;
            UseFutility = configurationProvider
                .GeneralConfiguration.UseFutility;
            FutilityDepth = configurationProvider
                .GeneralConfiguration.FutilityDepth;
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            InitializeFutilityMargins();
        }

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null);

        public virtual int Size => 0;

        public abstract int Search(int alpha, int beta, int depth);

        protected int Evaluate(int alpha, int beta)
        {
            int standPat = Position.GetValue();
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            var moves = Position.GetAllAttacks(Sorter);
            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                int score = -Evaluate(-beta, -alpha);

                Position.UnMake();

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        public override string ToString()
        {
            return $"{GetType().Name}";
        }

        public virtual bool IsBlocked()
        {
            return _isBlocked;
        }

        public virtual void ExecuteAsyncAction()
        {
            _isBlocked = true;
            Task.Factory.StartNew(() => { _isBlocked = false; });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckMoves(IMoveCollection moves, out Result result)
        {
            result = new Result();
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;

                return true;

            }

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                var value = Position.GetValue();
                if (value < ThreefoldRepetitionValue)
                {
                    result.GameResult = GameResult.ThreefoldRepetition;
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckMoves(int alpha, int beta, IMoveCollection moves, out int value)
        {
            value = 0;
            if (moves.Count == 0)
            {
                var lastMove = MoveHistory.GetLastMove();
                value = lastMove.IsCheck()
                    ? -EvaluationService.GetMateValue()
                    : -EvaluationService.Evaluate(Position);
                return true;
            }

            if (!MoveHistory.IsThreefoldRepetition(Position.GetKey())) return false;

            value = Position.GetValue();
            if (value < 0)
            {
                value += ThreefoldRepetitionValue;
            }
            else
            {
                value -= ThreefoldRepetitionValue;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IMoveCollection GenerateMoves(int alpha, int beta, int depth, IMove pv = null)
        {
            if (!UseFutility || depth > FutilityDepth || alpha <= -SearchValue || beta >= SearchValue)
                return Position.GetAllMoves(Sorter, pv);

            if (MoveHistory.GetLastMove().IsCheck()) return Position.GetAllMoves(Sorter, pv);

            var positionValue = Position.GetValue();

            if (positionValue + Margins[(byte)Position.GetPhase()][depth - 1] > alpha) return Position.GetAllMoves(Sorter, pv);

            var moves = Position.GetAllAttacks(Sorter);
            return moves.Count == 0 ? null : moves;
        }

        private void InitializeFutilityMargins()
        {
            Margins = new int[3][];

            var value1 = EvaluationService.GetValue(2, Phase.Opening);
            var value2 = EvaluationService.GetValue(3, Phase.Opening);
            var offset = EvaluationService.GetValue(0, Phase.Opening) / 2;
            var gap1 = value1 + offset;
            var gap2 = value2 + offset + offset;
            Margins[0] = new[] { gap1, gap2 };

            value1 = EvaluationService.GetValue(2, Phase.Middle);
            value2 = EvaluationService.GetValue(3, Phase.Middle);
            offset = EvaluationService.GetValue(0, Phase.Middle) / 2;
            gap1 = value1 + offset;
            gap2 = value2 + offset + offset;
            Margins[1] = new[] { gap1, gap2 };

            value1 = EvaluationService.GetValue(2, Phase.End);
            value2 = EvaluationService.GetValue(3, Phase.End);
            offset = EvaluationService.GetValue(0, Phase.End) / 2;
            gap1 = value1 + offset;
            gap2 = value2 + offset + offset;
            Margins[2] = new[] { gap1, gap2 };
        }
    }
}
