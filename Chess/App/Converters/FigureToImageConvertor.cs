using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Engine.Models.Enums;

namespace Kgb.ChessApp.Converters
{
    public class FigureToImageConvertor : IValueConverter
    {
        private readonly Dictionary<Piece, object> _images;

        public FigureToImageConvertor()
        {
            _images = new Dictionary<Piece, object>
            {
                [Piece.WhitePawn] = Application.Current.FindResource("WhitePawn"),
                [Piece.WhiteKnight] = Application.Current.FindResource("WhiteKnight"),
                [Piece.WhiteBishop] = Application.Current.FindResource("WhiteBishop"),
                [Piece.WhiteKing] = Application.Current.FindResource("WhiteKing"),
                [Piece.WhiteRook] = Application.Current.FindResource("WhiteRook"),
                [Piece.WhiteQueen] = Application.Current.FindResource("WhiteQueen"),
                [Piece.BlackPawn] = Application.Current.FindResource("BlackPawn"),
                [Piece.BlackKnight] = Application.Current.FindResource("BlackKnight"),
                [Piece.BlackBishop] = Application.Current.FindResource("BlackBishop"),
                [Piece.BlackKing] = Application.Current.FindResource("BlackKing"),
                [Piece.BlackRook] = Application.Current.FindResource("BlackRook"),
                [Piece.BlackQueen] = Application.Current.FindResource("BlackQueen")
            };
        }

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var f = value as Piece?;
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