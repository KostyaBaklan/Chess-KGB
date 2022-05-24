using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.End;

namespace Engine.Strategies.Base
{
    public abstract class StrategyBase : IStrategy
    {
        private bool _isBlocked;
        protected short Depth;
        protected int SearchValue;
        protected int ThreefoldRepetitionValue;
        protected int FutilityDepth;
        protected bool UseFutility;
        protected int MaxEndGameDepth;
        protected int[][] Margins;

        protected IPosition Position;
        protected IMoveSorter[] Sorters;

        protected IEvaluationService EvaluationService;
        protected readonly IMoveHistoryService MoveHistory;
        protected readonly IMoveProvider MoveProvider;
        protected readonly IMoveSorterProvider MoveSorterProvider;
        protected bool UseAging;
        private LmrNoCacheStrategy _endGameStrategy;

        protected LmrNoCacheStrategy EndGameStrategy
        {
            get { return _endGameStrategy ?? (_endGameStrategy = new LmrNoCacheStrategy(Depth, Position)); }
        }

        protected StrategyBase(short depth, IPosition position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            MaxEndGameDepth = configurationProvider
                .AlgorithmConfiguration.MaxEndGameDepth;
            SearchValue = configurationProvider
                .Evaluation.Static.Mate;
            ThreefoldRepetitionValue = configurationProvider
                .Evaluation.Static.ThreefoldRepetitionValue;
            UseFutility = configurationProvider
                .GeneralConfiguration.UseFutility;
            FutilityDepth = configurationProvider
                .GeneralConfiguration.FutilityDepth;
            UseAging = configurationProvider
                .GeneralConfiguration.UseAging;
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            MoveSorterProvider = ServiceLocator.Current.GetInstance<IMoveSorterProvider>();

            InitializeFutilityMargins();
        }

        public virtual int Size => 0;

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null);

        public abstract int Search(int alpha, int beta, int depth);

        protected void InitializeSorters(short depth, IPosition position, MoveSorter mainSorter)
        {
            Sorters = new IMoveSorter[depth + 2];

            var comparer = new HistoryComparer();
            var initialSorter = MoveSorterProvider.GetInitial(position, comparer);
            //Sorters[0] = new AttackSorter(position.GetBoard());
            Sorters[0] = MoveSorterProvider.GetBasic(position, comparer);

            var d = depth - 1;
            for (int i = 1; i < d; i++)
            {
                Sorters[i] = mainSorter;
            }

            for (var i = d; i < Sorters.Length; i++)
            {
                Sorters[i] = initialSorter;
            }
        }

        protected int Evaluate(int alpha, int beta)
        {
            int standPat = Position.GetValue();
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            var moves = Position.GetAllAttacks(Sorters[0]);
            for (var i = 0; i < moves.Length; i++)
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
        protected bool CheckMoves(MoveBase[] moves, out Result result)
        {
            result = new Result();
            if (moves.Length == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck ? GameResult.Mate : GameResult.Pat;
                return true;
            }

            if (Position.GetPhase() == Phase.Opening) return false;
            if (!MoveHistory.IsThreefoldRepetition(Position.GetKey())) return false;

            var value = Position.GetValue();
            if (value > 0) return false;

            result.GameResult = GameResult.ThreefoldRepetition;
            return true;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckMoves(int alpha, int beta, MoveBase[] moves, out int value)
        {
            value = 0;
            if (moves.Length == 0)
            {
                value = MoveHistory.GetLastMove().IsCheck
                    ? -EvaluationService.GetMateValue()
                    : Position.GetValue();
                return true;
            }

            if (Position.GetPhase() == Phase.Opening) return false;
            if (!MoveHistory.IsThreefoldRepetition(Position.GetKey())) return false;

            value = Position.GetValue();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveBase[] GeneratePvMoves(int alpha, int beta, int depth, MoveBase pv = null)
        {
            if (!UseFutility || depth > FutilityDepth || alpha <= -SearchValue || beta >= SearchValue)
                return Position.GetAllMoves(Sorters[Depth], pv);

            if (MoveHistory.GetLastMove().IsCheck) return Position.GetAllMoves(Sorters[Depth], pv);

            var positionValue = Position.GetValue();

            var i = Margins[(byte)Position.GetPhase()][depth - 1];
            if (positionValue + i > alpha) return Position.GetAllMoves(Sorters[Depth], pv);

            var moves = Position.GetAllAttacks(Sorters[depth]);
            return moves.Length == 0 ? null : moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveBase[] GenerateMoves(int alpha, int beta, int depth, MoveBase pv = null)
        {
            if (!UseFutility || depth > FutilityDepth || alpha <= -SearchValue || beta >= SearchValue)
                return Position.GetAllMoves(Sorters[depth], pv);

            if (MoveHistory.GetLastMove().IsCheck) return Position.GetAllMoves(Sorters[depth], pv);

            var positionValue = Position.GetValue();

            var i = Margins[(byte)Position.GetPhase()][depth - 1];
            if (positionValue + i > alpha) return Position.GetAllMoves(Sorters[depth], pv);

            var moves = Position.GetAllAttacks(Sorters[depth]);
            return moves.Length == 0 ? null : moves;
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
