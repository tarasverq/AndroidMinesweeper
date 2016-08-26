namespace Android_Mines
{
    internal enum CellState
    {
        Closed,
        Opened
    }
    internal enum MineState
    {
        No,
        Yes
    }

    class Cell
    {
        public CellState CellState { get; set; }
        public MineState MineState { get; set; }
        public int MinesAround { get; set; }
        public Cell()
        {
            CellState = CellState.Closed;
            MineState = MineState.No;
        }
    }
}