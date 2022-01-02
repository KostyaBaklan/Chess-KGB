namespace Infrastructure.Models.Enums
{
    public static class FigureKindExtensions
    {
        public static bool IsWhite(this FigureKind figure)
        {
            return (int) figure < 6;
        }
        public static bool IsBlack(this FigureKind figure)
        {
            return (int)figure > 5;
        }
    }
}