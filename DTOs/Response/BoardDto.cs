namespace OthelloAPI.DTOs.Response;

public class BoardDto
{
    public int Size { get; set; }
    public List<List<CellDto>> Cells { get; set; } = new();
}

// using OthelloAPI.DTOs;

// public class BoardResponseDto
// {
//     public Board Board { get; set; } = default!;
// }
