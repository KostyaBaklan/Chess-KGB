using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
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
        protected bool UseRazoring;
        protected int RazoringDepth;
        protected bool UseSortHard;
        protected bool UseSortDifference;
        protected int MaxEndGameDepth;
        protected int[] SortDepth;
        protected int[] SortHardDepth;
        protected int[] SortDifferenceDepth;
        protected int[][] FutilityMargins;
        protected int[] RazoringMargins;

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
            var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
            var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
            var generalConfiguration = configurationProvider.GeneralConfiguration;

            MaxEndGameDepth = algorithmConfiguration.MaxEndGameDepth;
            SortDepth = sortingConfiguration.SortDepth;
            SortHardDepth = sortingConfiguration.SortHardDepth;
            SortDifferenceDepth = sortingConfiguration.SortDifferenceDepth;
            SearchValue = configurationProvider.Evaluation.Static.Mate;
            ThreefoldRepetitionValue = configurationProvider.Evaluation.Static.ThreefoldRepetitionValue;
            UseFutility = generalConfiguration.UseFutility;
            FutilityDepth = generalConfiguration.FutilityDepth;
            UseRazoring = generalConfiguration.UseRazoring;
            RazoringDepth = generalConfiguration.RazoringDepth;
            UseAging = generalConfiguration.UseAging;
            UseSortHard = sortingConfiguration.UseSortHard;
            UseSortDifference = sortingConfiguration.UseSortDifference;
            Depth = depth;
            Position = position;
            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            MoveSorterProvider = ServiceLocator.Current.GetInstance<IMoveSorterProvider>();

            InitializeMargins();
        }

        public virtual int Size => 0;

        public abstract IResult GetResult();

        public abstract IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null);

        public abstract int Search(int alpha, int beta, int depth);

        protected virtual void InitializeSorters(short depth, IPosition position, MoveSorter mainSorter)
        {
            Sorters = new IMoveSorter[depth + 2];

            var initialSorter = MoveSorterProvider.GetInitial(position, Sorting.Sort.HistoryComparer);
            Sorters[0] = MoveSorterProvider.GetBasic(position, Sorting.Sort.HistoryComparer);

            var d = depth - SortDepth[depth];

            if (UseSortHard)
            {
                var hardExtended = MoveSorterProvider.GetHardExtended(position, Sorting.Sort.HistoryComparer);
                var hard = d - SortHardDepth[depth];
                for (int i = 1; i < hard; i++)
                {
                    Sorters[i] = mainSorter;
                }

                for (var i = hard; i < d; i++)
                {
                    Sorters[i] = hardExtended;
                }
            }
            else if (UseSortDifference)
            {
                var differenceExtended =
                    MoveSorterProvider.GetDifferenceExtended(position, Sorting.Sort.HistoryComparer);
                var x = 1 + SortDifferenceDepth[depth];
                for (int i = 1; i < x; i++)
                {
                    Sorters[i] = differenceExtended;
                }

                for (int i = x; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
            }
            else
            {
                for (int i = 1; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
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
        protected bool CheckMoves(int count, out Result result)
        {
            result = new Result();
            if (count == 0)
            {
                result.GameResult = MoveHistory.IsLastMoveWasCheck() ? GameResult.Mate : GameResult.Pat;
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
        protected bool CheckPosition(int count, out int value)
        {
            value = 0;
            if (count == 0)
            {
                value = MoveHistory.IsLastMoveWasCheck()
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
        protected MoveBase[] GenerateMoves(int alpha, int beta, int depth, MoveBase pv = null)
        {
            if (!UseFutility || depth > FutilityDepth)
                return Position.GetAllMoves(Sorters[depth], pv);

            if (MoveHistory.IsLastMoveWasCheck()) return Position.GetAllMoves(Sorters[depth], pv);

            var positionValue = Position.GetValue();

            var i = FutilityMargins[(byte) Position.GetPhase()][depth - 1];
            if (positionValue + i > alpha) return Position.GetAllMoves(Sorters[depth], pv);

            var moves = Position.GetAllAttacks(Sorters[depth]);
            return moves.Length == 0 ? null : moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int AdjustDepth(int alpha, int depth)
        {
            if (!UseRazoring || depth != RazoringDepth || MoveHistory.IsLastMoveWasCheck()) return depth;

            var positionValue = Position.GetValue();

            var i = RazoringMargins[(byte) Position.GetPhase()];
            if (positionValue + i <= alpha)
            {
                return depth - 1;
            }

            return depth;
        }

        private void InitializeMargins()
        {
            FutilityMargins = new int[3][];

            var value1 = EvaluationService.GetValue(2, Phase.Opening);
            var value2 = EvaluationService.GetValue(3, Phase.Opening);
            var offset = EvaluationService.GetValue(0, Phase.Opening) / 2;
            var gap1 = value1;
            var gap2 = value2 + offset;
            FutilityMargins[0] = new[] {gap1, gap2};

            value1 = EvaluationService.GetValue(2, Phase.Middle);
            value2 = EvaluationService.GetValue(3, Phase.Middle);
            offset = EvaluationService.GetValue(0, Phase.Middle) / 2;
            gap1 = value1;
            gap2 = value2 + offset;
            FutilityMargins[1] = new[] {gap1, gap2};

            value1 = EvaluationService.GetValue(2, Phase.End);
            value2 = EvaluationService.GetValue(3, Phase.End);
            offset = EvaluationService.GetValue(0, Phase.End) / 2;
            gap1 = value1;
            gap2 = value2 + offset;
            FutilityMargins[2] = new[] {gap1, gap2};

            RazoringMargins = new[]
            {
                EvaluationService.GetValue(4, Phase.Opening) - EvaluationService.GetValue(0, Phase.Opening) / 2,
                EvaluationService.GetValue(4, Phase.Middle) - EvaluationService.GetValue(0, Phase.Middle) / 2,
                EvaluationService.GetValue(4, Phase.End) - EvaluationService.GetValue(0, Phase.End) / 2
            };
        }
    }
}
