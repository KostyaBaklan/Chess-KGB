using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Aspiration.LateMove;
using Engine.Strategies.Aspiration.Null;
using Engine.Strategies.Base;
using Engine.Strategies.LateMove.Base.Null;
using Engine.Strategies.LateMove.Deep;
using Engine.Strategies.LateMove.Deep.Null;
using Engine.Strategies.PVS;
using Kgb.ChessApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Kgb.ChessApp.Views
{
    public class GameViewModel : BindableBase, INavigationAware
    {
        private short _level;
        private readonly double _blockTimeout;
        private Turn _turn = Turn.White;
        private List<MoveBase> _moves;

        private readonly IPosition _position;
        private StrategyBase _strategy;
        private readonly Dictionary<string, CellViewModel> _cellsMap;

        private readonly IMoveFormatter _moveFormatter;
        private readonly IEvaluationService _evaluationService;
        private readonly IMoveHistoryService _moveHistoryService;

        public GameViewModel(IMoveFormatter moveFormatter)
        {
            _blockTimeout = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .GeneralConfiguration.BlockTimeout;
            _moveFormatter = moveFormatter;
            _cellsMap = new Dictionary<string, CellViewModel>(64);
            for (int i = 0; i < 64; i++)
            {
                var x = i / 8;
                var y = i % 2;
                CellType cellType;

                if (x % 2 == 0)
                {
                    cellType = y == 0 ? CellType.Black : CellType.White;
                }
                else
                {
                    cellType = y == 1 ? CellType.Black : CellType.White;
                }
                var square = new Square(i);
                CellViewModel cell = new CellViewModel{Cell = square, CellType =cellType};
                _cellsMap[square.AsString()] = cell;
            }

            FillCells();

            _position = new Position();

            MoveItems = new ObservableCollection<MoveModel>();

            SelectionCommand = new DelegateCommand<CellViewModel>(SelectionCommandExecute, SelectionCommandCanExecute);
            UndoCommand = new DelegateCommand(UndoCommandExecute);
            SaveHistoryCommand = new DelegateCommand(SaveHistoryCommandExecute);
            _evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        private void FillCells()
        {
            _cellsMap["A1"].Figure = Piece.WhiteRook;
            _cellsMap["B1"].Figure = Piece.WhiteKnight;
            _cellsMap["C1"].Figure = Piece.WhiteBishop;
            _cellsMap["D1"].Figure = Piece.WhiteQueen;
            _cellsMap["E1"].Figure = Piece.WhiteKing;
            _cellsMap["F1"].Figure = Piece.WhiteBishop;
            _cellsMap["G1"].Figure = Piece.WhiteKnight;
            _cellsMap["H1"].Figure = Piece.WhiteRook;

            _cellsMap["A2"].Figure = Piece.WhitePawn;
            _cellsMap["B2"].Figure = Piece.WhitePawn;
            _cellsMap["C2"].Figure = Piece.WhitePawn;
            _cellsMap["D2"].Figure = Piece.WhitePawn;
            _cellsMap["E2"].Figure = Piece.WhitePawn;
            _cellsMap["F2"].Figure = Piece.WhitePawn;
            _cellsMap["G2"].Figure = Piece.WhitePawn;
            _cellsMap["H2"].Figure = Piece.WhitePawn;

            _cellsMap["A7"].Figure = Piece.BlackPawn;
            _cellsMap["B7"].Figure = Piece.BlackPawn;
            _cellsMap["C7"].Figure = Piece.BlackPawn;
            _cellsMap["D7"].Figure = Piece.BlackPawn;
            _cellsMap["E7"].Figure = Piece.BlackPawn;
            _cellsMap["F7"].Figure = Piece.BlackPawn;
            _cellsMap["G7"].Figure = Piece.BlackPawn;
            _cellsMap["H7"].Figure = Piece.BlackPawn;

            _cellsMap["A8"].Figure = Piece.BlackRook;
            _cellsMap["B8"].Figure = Piece.BlackKnight;
            _cellsMap["C8"].Figure = Piece.BlackBishop;
            _cellsMap["D8"].Figure = Piece.BlackQueen;
            _cellsMap["E8"].Figure = Piece.BlackKing;
            _cellsMap["F8"].Figure = Piece.BlackBishop;
            _cellsMap["G8"].Figure = Piece.BlackKnight;
            _cellsMap["H8"].Figure = Piece.BlackRook;
        }

        private bool _useMachine;

        public bool UseMachine
        {
            get => _useMachine;
            set => SetProperty(ref _useMachine, value);
        }

        private IEnumerable<int> _numbers;

        public IEnumerable<int> Numbers
        {
            get => _numbers;
            set => SetProperty(ref _numbers, value);
        }

        private IEnumerable<string> _labels;

        public IEnumerable<string> Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);
        }

        private IEnumerable<CellViewModel> _cells;

        public IEnumerable<CellViewModel> Cells
        {
            get => _cells;
            set => SetProperty(ref _cells, value);
        }

        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ObservableCollection<MoveModel> MoveItems { get; }

        public ICommand SelectionCommand { get; }

        public ICommand UndoCommand { get; }

        public ICommand SaveHistoryCommand { get; }

        #region Implementation of INavigationAware

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var numbers = new[] {1, 2, 3, 4, 5, 6, 7, 8};
            var labels = new[] { "A","B" ,"C","D","E","F","G","H"};
            List<CellViewModel> models = new List<CellViewModel>(64);
            var color = navigationContext.Parameters.GetValue<string>("Color");

            var level = navigationContext.Parameters.GetValue<short>("Level");
            _evaluationService.Initialize(level);
            _strategy = new LmrDeepExtendedStrategy(level, _position);
            _level = level;
            Title = $"Strategy={_strategy}, Level={level}";

            if (color == "White")
            {
                var array = numbers.Reverse().ToArray();
                Numbers = array;
                Labels = labels;

                foreach (var n in array)
                {
                    foreach (var l in labels)
                    {
                        var model = _cellsMap[$"{l}{n}"];
                        models.Add(model);
                    }
                }

                Cells = models;
            }
            else
            {
                var array = labels.Reverse().ToArray();
                Numbers = numbers;
                Labels = array;

                foreach (var n in numbers)
                {
                    foreach (var l in array)
                    {
                        var model = _cellsMap[$"{l}{n}"];
                        models.Add(model);
                    }
                }

                Cells = models;

                MakeMachineMove();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion

        private void SaveHistoryCommandExecute()
        {
            IEnumerable<MoveBase> history = _position.GetHistory();
            List<string> moves = new List<string>();
            bool isWhite = true;
            StringBuilder builder = new StringBuilder();
            foreach (var move in history)
            {
                if (isWhite)
                {
                    builder = new StringBuilder();
                    builder.Append($"W={_moveFormatter.Format(move)} ");
                }
                else
                {
                    builder.Append($"B={_moveFormatter.Format(move)} ");
                    moves.Add(builder.ToString());
                }
                isWhite = !isWhite;
            }
            var path = "History";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllLines($@"{path}\\{DateTime.Now:yyyy_MM_dd_hh_mm_ss}.txt", moves);
        }

        private void UndoCommandExecute()
        {
            if (!MoveItems.Any()) return;

            Zero();

            _position.UnMake();

            var moveModel = MoveItems.Last();
            if (string.IsNullOrEmpty(moveModel.Black))
            {
                MoveItems.Remove(moveModel);
            }
            else
            {
                moveModel.Black = null;
            }

            UpdateView();

            _turn = _position.GetTurn();
        }

        private bool SelectionCommandCanExecute(CellViewModel arg)
        {
            if (arg.State == State.MoveTo || arg.State == State.MoveFrom) return true;

            return arg.Figure != null && (_turn == Turn.White ? arg.Figure.Value.IsWhite() : arg.Figure.Value.IsBlack());
        }

        private void SelectionCommandExecute(CellViewModel cellViewModel)
        {
            switch (cellViewModel.State)
            {
                case State.Idle:
                    Zero();

                    IEnumerable<MoveBase> possibleMoves = GetAllMoves(cellViewModel.Cell,cellViewModel.Figure.Value);
                    _moves = possibleMoves.ToList();
                    if (_moves.Any())
                    {
                        foreach (var m in _moves)
                        {
                            _cellsMap[m.To.AsString()].State = State.MoveTo;
                        }

                        cellViewModel.State = State.MoveFrom;
                    }
                    break;
                case State.MoveFrom:

                    Zero();
                    break;
                case State.MoveTo:

                    Zero();

                    var move = _moves.FirstOrDefault(m => m.To.Equals(cellViewModel.Cell));
                    if (move == null) return;

                    MakeMove(move);

                    //_timer.Start();
                    //var q = _strategy.GetResult().Move;
                    //_timer.Stop();

                    //MessageBox.Show($"Elapsed = {_timer.Elapsed} !");

                    //if (q != null)
                    //{
                    //    MakeMove(q);
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"No Moves");
                    //}

                    MakeMachineMove();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MakeMachineMove()
        {
            if (!_useMachine) return;

            Task.Delay(10)
                .ContinueWith(t =>
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    while (_strategy.IsBlocked())
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(_blockTimeout));
                    }

                    var q = _strategy.GetResult();

                    _strategy.ExecuteAsyncAction();
                    timer.Stop();
                    return new Tuple<IResult, TimeSpan>(q, timer.Elapsed);
                })
                .ContinueWith(t =>
                    {
                        Tuple<IResult, TimeSpan> tResult = null;
                        try
                        {
                            tResult = t.Result;
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show($"Error = {exception} !");
                        }
                        if (tResult != null)
                        {
                            MessageBox.Show($"Elapsed = {tResult.Item2} !");
                            switch (t.Result.Item1.GameResult)
                            {
                                case GameResult.Continue:
                                    MakeMove(tResult.Item1.Move, tResult.Item2);
                                    break;
                                case GameResult.Pat:
                                    MessageBox.Show("Pat !!!");
                                    break;
                                case GameResult.ThreefoldRepetition:
                                    MessageBox.Show("Threefold Repetition !!!");
                                    break;
                                case GameResult.Mate:
                                    MessageBox.Show("Mate !!!");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        else
                        {
                            MessageBox.Show($"No Moves");
                        }
                    },
                    CancellationToken.None, TaskContinuationOptions.LongRunning,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private IEnumerable<MoveBase> GetAllMoves(Square cell, Piece piece)
        {
            return _position.GetAllAttacks(cell, piece).Concat(_position.GetAllMoves(cell, piece));
        }

        private void MakeMove(MoveBase move, TimeSpan? time = null)
        {
            _position.Make(move);

            var lastModel = MoveItems.LastOrDefault();
            MoveModel mm = lastModel;
            if (lastModel == null)
            {
                var model = new MoveModel
                {
                    Number = 1,
                    White = $" {_moveFormatter.Format(move)} ",
                    WhiteValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()} "
                };
                MoveItems.Add(model);
                mm = model;
            }
            else
            {
                if (_turn == Turn.White)
                {
                    var model = new MoveModel
                    {
                        Number = lastModel.Number + 1,
                        White = $" {_moveFormatter.Format(move)} ",
                        WhiteValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()} "
                    };
                    MoveItems.Add(model);
                    mm = model;
                }
                else
                {
                    lastModel.Black = $" {_moveFormatter.Format(move)} ";
                    lastModel.BlackValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()} ";
                    var process = Process.GetCurrentProcess();
                    lastModel.Memory = $" {process.WorkingSet64 / 1024/1024} MB";
                    lastModel.Evaluation = _evaluationService.Size;
                    lastModel.Table = _strategy.Size;
                }
            }

            if (time != null)
            {
                mm.Time = time.Value;
            }

            UpdateView();

            _turn = _position.GetTurn();

            Thread.Sleep(100);
        }

        private void UpdateView()
        {
            foreach (var cell in _cells)
            {
                _position.GetPiece(cell.Cell,out var piece);
                _cellsMap[cell.Cell.AsString()].Figure = piece;
            }
        }

        private void Zero()
        {
            foreach (var viewModel in _cellsMap.Values)
            {
                viewModel.State = State.Idle;
            }
        }
    }
}
