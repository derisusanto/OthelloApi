
namespace OthelloAPI.Models;

public class Piece
{
    public PieceColor Color {get; set;}
    public Piece(PieceColor color)
    {
        Color = color;
    }

}