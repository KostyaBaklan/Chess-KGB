namespace Engine.Models.Boards
{
    //public struct PieceBoard
    //{
    //    private BitBoard _board;

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Add(byte rank)
    //    {
    //        _board = _board.Set(rank);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Remove(byte rank)
    //    {
    //        _board = _board.Off(rank);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public IEnumerable<Square> Items()
    //    {
    //        return _board.BitScan().Select(c => new Square(c));
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public IEnumerable<int> Coordinates()
    //    {
    //        return _board.BitScan();
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Set(params int[] items)
    //    {
    //        _board = _board.Set(items);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void Replace(byte from, byte to)
    //    {
    //        _board = _board.Off(from).Set(to);
    //    }
    //}
}