using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Algorithms.Interfaces;
using Algorithms.Strategies.Scout;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;
using Infrastructure.Positions;
using UI.Test.Models;

namespace UI.Test.ViewModels
{
    class BoardViewModel:ViewModelBase
    {
        private Turn _turn = Turn.White;
        private readonly IPosition _position;
        private readonly Dictionary<Coordinate, CellViewModel> _cells;
        private List<IMove> _moves;
        private readonly IMoveFormatter _moveFormatter;
        private readonly IStrategy _strategy;
        private readonly Stopwatch _timer;
        private IOpennigService _openning;

        public BoardViewModel()
        {
            _timer = new Stopwatch();
            Cells = new ObservableCollection<CellViewModel>();
            SelectionCommand = new RelayCommand<CellViewModel>(SelectionCommandExecute, SelectionCommandCanExecute);
            UndoCommand = new RelayCommand(UndoCommandExecute);
            SaveCommand = new RelayCommand(SaveCommandExecute);
            NextCommand = new RelayCommand(NextCommandExecute);
            StartCommand = new RelayCommand(StartCommandExecute);

            var coordinateProvider = ServiceLocator.Current.GetInstance<ICoordinateProvider>();
            _position = new Position();
            _cells = new Dictionary<Coordinate, CellViewModel>();
            Moves = new ObservableCollection<string>();

            for (int x = 0; x < 8; x++)
            {
                CellType type = x%2 == 0? CellType.White: CellType.Black;
                for (int y = 7; y > -1; y--)
                {
                    var cell = coordinateProvider.GetCoordinate(x, y);

                    CellViewModel model = new CellViewModel {CellType = type, Cell = cell};

                    if (y == 1)
                    {
                        model.Figure = FigureKind.WhitePawn;
                    }
                    else if (y == 6)
                    {
                        model.Figure = FigureKind.BlackPawn;
                    }
                    else if (y == 0)
                    {
                        if(x == 0||x==7)
                            model.Figure = FigureKind.WhiteRook;
                        else if(x == 1 || x == 6)
                            model.Figure = FigureKind.WhiteKnight;
                        else if (x == 2 || x == 5)
                            model.Figure = FigureKind.WhiteBishop;
                        else if (x == 3)
                            model.Figure = FigureKind.WhiteQueen;
                        else
                            model.Figure = FigureKind.WhiteKing;
                    }
                    else if (y == 7)
                    {
                        if (x == 0 || x == 7)
                            model.Figure = FigureKind.BlackRook;
                        else if (x == 1 || x == 6)
                            model.Figure = FigureKind.BlackKnight;
                        else if (x == 2 || x == 5)
                            model.Figure = FigureKind.BlackBishop;
                        else if (x == 3)
                            model.Figure = FigureKind.BlackQueen;
                        else
                            model.Figure = FigureKind.BlackKing;
                    }

                    type = type == CellType.Black ? CellType.White : CellType.Black;

                    Cells.Add(model);

                    _cells[cell] = model;
                }
            }

            _moveFormatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            _openning = ServiceLocator.Current.GetInstance<IOpennigService>();
            _strategy = new ScoutFigureValueStrategy(6, _position);
        }

        public ObservableCollection<CellViewModel> Cells { get; }

        public ICommand SelectionCommand { get; }

        public ICommand UndoCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand NextCommand { get; }

        public ICommand StartCommand { get; }

        public ObservableCollection<string> Moves { get; }

        private void StartCommandExecute()
        {
            throw new NotImplementedException();
        }

        private void NextCommandExecute()
        {
            throw new NotImplementedException();
        }

        private void SaveCommandExecute()
        {
            //var moves = "Moves";
            //if (!Directory.Exists(moves))
            //{
            //    Directory.CreateDirectory(moves);
            //}

            //var file = Path.Combine(moves, $"{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.moves");
            //File.WriteAllLines(file,Moves);
            //_openning.Save(Moves);
        }

        private void UndoCommandExecute()
        {
            if (!Moves.Any()) return;

            Zero();

            _position.UnMake();

            Moves.RemoveAt(Moves.Count - 1);

            UpdateView();

            _turn = _position.Turn;
        }

        private void SelectionCommandExecute(CellViewModel cellViewModel)
        {
            switch (cellViewModel.State)
            {
                case State.Idle:
                    Zero();

                    IEnumerable<IMove> possibleMoves =GetAllMoves(cellViewModel.Cell);
                    _moves = possibleMoves.ToList();
                    if (_moves.Any())
                    {
                        foreach (var m in _moves)
                        {
                            _cells[m.To].State = State.MoveTo;
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

                    Task.Delay(10)
                        .ContinueWith(t =>
                        {
                            var timer = new Stopwatch();
                            timer.Start();
                            var q = _strategy.GetResult().Move;
                            timer.Stop();
                            return new Tuple<IMove,TimeSpan>(q,timer.Elapsed);
                        })
                        .ContinueWith(t =>
                            {
                                var tResult = t.Result;
                                if (tResult != null)
                                {
                                    MessageBox.Show($"Elapsed = {tResult.Item2} !");
                                    MakeMove(tResult.Item1);
                                }
                                else
                                {
                                    MessageBox.Show($"No Moves");
                                }
                            },
                            CancellationToken.None, TaskContinuationOptions.LongRunning,
                            TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable<IMove> GetAllMoves(Coordinate cell)
        {
            foreach (var move in _position.GetAllAttacks(cell).Concat(_position.GetAllMoves(cell)))
            {
                _position.Make(move);

                var isCheck = _position.IsCheck();

                _position.UnMake();

                if (!isCheck)
                    yield return move;
            }
        }

        private void MakeMove(IMove move)
        {
            _position.Make(move);

            Moves.Add($" [{_moveFormatter.Format(move)}][{-_position.GetValue()}] ");

            UpdateView();

            _turn = _position.Turn;

            Thread.Sleep(100);
        }

        private void UpdateView()
        {
            foreach (var cell in _cells.Keys)
            {
                _cells[cell].Figure = _position.GetFigure(cell);
            }
        }

        private void Zero()
        {
            foreach (var viewModel in _cells.Values)
            {
                viewModel.State = State.Idle;
            }
        }

        private bool SelectionCommandCanExecute(CellViewModel arg)
        {
            if (arg.State == State.MoveTo || arg.State == State.MoveFrom) return true;

            return arg.Figure != null && (_turn == Turn.White ? arg.Figure.Value.IsWhite() : arg.Figure.Value.IsBlack());
        }
    }
}
