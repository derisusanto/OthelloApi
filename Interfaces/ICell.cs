using OthelloAPI.Models;
public interface ICell
{
    Position Position { get; }
    Piece? Piece { get; set; }
}
