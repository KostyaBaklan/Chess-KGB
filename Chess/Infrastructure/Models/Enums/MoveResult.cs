namespace Infrastructure.Models.Enums
{
    public enum MoveResult
    {
        Mate = 0,
        TieNoMatePossible,
        EatQueenCheck,
        EatQueen,
        PawnPromotionToQueen,
        EatRookCheck,
        EatRook,
        PawnPromotionToRook,
        EatKnightCheck,
        EatKnight,
        PawnPromotionToKnight,
        EatBishopCheck,
        EatBishop,
        PawnPromotionToBishop,
        EatPawnCheck,
        EatPawn,
        Check,
        Normal,
        Repeat,
        OverCell
    }
}
