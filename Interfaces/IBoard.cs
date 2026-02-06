using OthelloAPI.Models;
public interface IBoard
{
    int Size { get; }
    Cell[,] Cells { get; }
}
