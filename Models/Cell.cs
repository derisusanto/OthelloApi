namespace OthelloAPI.Models;

public class Cell : ICell
{
    public Position  Position {get; set;}

    public Piece? Piece {get; set;}
    public Cell(Position position)
    {
        Position = position;
        Piece = null;
    }
}