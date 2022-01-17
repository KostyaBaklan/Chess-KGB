using System;
using Prism.Mvvm;

namespace Kgb.ChessApp.Models
{
    public class MoveModel:BindableBase
    {
        private int _number;

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private string _white;

        public string White
        {
            get => _white;
            set => SetProperty(ref _white, value);
        }

        private string _black;

        public string Black
        {
            get => _black;
            set => SetProperty(ref _black, value);
        }

        private string _memory;

        public string Memory
        {
            get => _memory;
            set => SetProperty(ref _memory, value);
        }

        private TimeSpan _time;

        public TimeSpan Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private int _evaluation;

        public int Evaluation
        {
            get => _evaluation;
            set => SetProperty(ref _evaluation, value);
        }

        private int _table;

        public int Table
        {
            get => _table;
            set => SetProperty(ref _table, value);
        }
    }
}