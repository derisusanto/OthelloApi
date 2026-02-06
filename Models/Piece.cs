
namespace OthelloAPI.Models;

public class Piece : IPiece
{
    public PieceColor Color {get; set;}
    public Piece(PieceColor color)
    {
        Color = color;
    }

}