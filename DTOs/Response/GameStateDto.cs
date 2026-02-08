namespace OthelloAPI.DTOs.Response;
public class GameStateDto
{
    public BoardDto Board { get; set; }
    public ScoreDto Score { get; set; } = new();
    public PlayerDto CurrentPlayer { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new(); // tambahkan
    public List<PositionDto> ValidMoves { get; set; } // harus List<PositionDto>
    public bool IsGameOver { get; set; }
}

