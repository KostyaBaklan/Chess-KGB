using GalaSoft.MvvmLight;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;
using UI.Test.Models;

namespace UI.Test.ViewModels
{
    class CellViewModel : ViewModelBase
    {
        public CellViewModel()
        {
            _state = State.Idle;
        }

        private State _state;

        public State State
        {
            get => _state;
            set => Set(()=>State, ref _state,value);
        }

        private CellType _cellType;

        public CellType CellType
        {
            get => _cellType;
            set => Set(() => CellType, ref _cellType, value);
        }

        private FigureKind? _figure;

        public FigureKind? Figure
        {
            get => _figure;
            set => Set(() => Figure, ref _figure, value);
        }

        public Coordinate Cell { get; set; }
    }
}