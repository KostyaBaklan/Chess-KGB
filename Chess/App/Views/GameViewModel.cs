using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Strategies;
using Engine.Strategies.AlphaBeta;
using Kgb.ChessApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Kgb.ChessApp.Views
{
    public class GameViewModel : BindableBase, INavigationAware
    {
        private Turn _turn = Turn.White;
        private readonly IPosition _position;
        private List<IMove> _moves;
        private readonly IStrategy _strategy;
        private readonly Dictionary<string, CellViewModel> _cellsMap;
        private readonly IMoveFormatter _moveFormatter;

        public GameViewModel(IMoveFormatter moveFormatter)
        {
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

            _position = new Position();

            Moves = new ObservableCollection<string>();

            SelectionCommand = new DelegateCommand<CellViewModel>(SelectionCommandExecute, SelectionCommandCanExecute);
            UndoCommand = new DelegateCommand(UndoCommandExecute);
            SaveHistoryCommand = new DelegateCommand(SaveHistoryCommandExecute);

            _strategy = new AlphaBetaDifferenceStrategy(5,_position);
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

        public ObservableCollection<string> Moves { get; }

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
            IEnumerable<IMove> history = _position.GetHistory();
            File.WriteAllLines($"History_{DateTime.Now:yyyy_MM_dd_hh_mm_ss}.txt",history.Select(_moveFormatter.Format));
        }

        private void UndoCommandExecute()
        {
            if (!Moves.Any()) return;

            Zero();

            _position.UnMake();

            Moves.RemoveAt(Moves.Count - 1);

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

                    IEnumerable<IMove> possibleMoves = GetAllMoves(cellViewModel.Cell,cellViewModel.Figure.Value);
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
            Task.Delay(10)
                .ContinueWith(t =>
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    var q = _strategy.GetResult().Move;
                    timer.Stop();
                    return new Tuple<IMove, TimeSpan>(q, timer.Elapsed);
                })
                .ContinueWith(t =>
                    {
                        Tuple<IMove, TimeSpan> tResult = null;
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
                            MakeMove(tResult.Item1);
                        }
                        else
                        {
                            MessageBox.Show($"No Moves");
                        }
                    },
                    CancellationToken.None, TaskContinuationOptions.LongRunning,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private IEnumerable<IMove> GetAllMoves(Square cell, Piece piece)
        {
            foreach (var move in _position.GetAllAttacks(cell, piece).Concat(_position.GetAllMoves(cell, piece)))
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
