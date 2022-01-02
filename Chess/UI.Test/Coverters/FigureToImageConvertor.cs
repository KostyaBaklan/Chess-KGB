using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Infrastructure.Models.Enums;

namespace UI.Test.Coverters
{
    public class FigureToImageConvertor : IValueConverter
    {
        private readonly Dictionary<FigureKind, object> _images;

        public FigureToImageConvertor()
        {
            _images = new Dictionary<FigureKind, object>
            {
                [FigureKind.WhitePawn] = Application.Current.FindResource("WhitePawn"),
                [FigureKind.WhiteKnight] = Application.Current.FindResource("WhiteKnight"),
                [FigureKind.WhiteBishop] = Application.Current.FindResource("WhiteBishop"),
                [FigureKind.WhiteKing] = Application.Current.FindResource("WhiteKing"),
                [FigureKind.WhiteRook] = Application.Current.FindResource("WhiteRook"),
                [FigureKind.WhiteQueen] = Application.Current.FindResource("WhiteQueen"),
                [FigureKind.BlackPawn] = Application.Current.FindResource("BlackPawn"),
                [FigureKind.BlackKnight] = Application.Current.FindResource("BlackKnight"),
                [FigureKind.BlackBishop] = Application.Current.FindResource("BlackBishop"),
                [FigureKind.BlackKing] = Application.Current.FindResource("BlackKing"),
                [FigureKind.BlackRook] = Application.Current.FindResource("BlackRook"),
                [FigureKind.BlackQueen] = Application.Current.FindResource("BlackQueen")
            };
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var f = value as FigureKind?;
            if (f == null) return null;

            return _images[f.Value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}