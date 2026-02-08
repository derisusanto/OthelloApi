namespace OthelloAPI.DTOs.Response;
public class CellDto
    {
        public PositionDto Position { get; set; } = new();
        public PieceDto? Piece { get; set; }
    }
