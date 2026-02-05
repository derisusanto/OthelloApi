
using Microsoft.AspNetCore.Mvc;
using OthelloAPI.Models;
using OthelloAPI.Services;

namespace OthelloAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OthelloController : ControllerBase
    {
        private static GameController? _game;

        public class PieceDto { public string Color { get; set; } = "Empty"; }
        public class PlayerDto { public string Name { get; set; } = ""; public string Color { get; set; } = ""; }
        
        [HttpPost("start")]
        public IActionResult StartGame([FromBody] List<string> playerNames)
        {
            if (playerNames.Count != 2) return BadRequest("Harus ada 2 pemain");

            var players = new List<Player>
            {
                new Player(playerNames[0], PlayerColor.Black),
                new Player(playerNames[1], PlayerColor.White)
            };

            _game = new GameController(players);

            return Ok(new
            {
                message = "Game started",
                score = new { Black = _game.GetScore().Black, White = _game.GetScore().White },
                currentPlayer = ToDto(_game.CurrentPlayer),
                validMoves = GetValidMoves(_game)
            });
        }

        [HttpGet("board")]
        public IActionResult GetBoard()
        {
            if (_game == null) return BadRequest("Game belum dimulai");
            return Ok(GetBoardJagged(_game));
        }

        [HttpPost("play")]
        public IActionResult Play([FromBody] Position pos)
        {
            if (_game == null) return BadRequest("Game belum dimulai");

            if (!_game.PlayAt(pos)) return BadRequest("Invalid move / Cell sudah terisi");

            return Ok(new
            {
                board = GetBoardJagged(_game),
                currentPlayer = ToDto(_game.CurrentPlayer),
                score = new { Black = _game.GetScore().Black, White = _game.GetScore().White },
                validMoves = GetValidMoves(_game),
                isGameOver = _game.IsGameOver
            });
        }

        [HttpPost("pass")]
        public IActionResult Pass()
        {
            if (_game == null) return BadRequest("Game belum dimulai");

            _game.PassTurn();

            return Ok(new
            {
                board = GetBoardJagged(_game),
                currentPlayer = ToDto(_game.CurrentPlayer),
                score = new { Black = _game.GetScore().Black, White = _game.GetScore().White },
                validMoves = GetValidMoves(_game),
                isGameOver = _game.IsGameOver
            });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            if (_game == null) return BadRequest("Game belum dimulai");

            return Ok(new
            {
                board = GetBoardJagged(_game),
                currentPlayer = ToDto(_game.CurrentPlayer),
                score = new { Black = _game.GetScore().Black, White = _game.GetScore().White },
                validMoves = GetValidMoves(_game),
                isGameOver = _game.IsGameOver
            });
        }

        // Helper: convert Player ke DTO
        private PlayerDto ToDto(Player p)
            => new PlayerDto { Name = p.Name, Color = p.Color.ToString() };

        // Helper: buat board jagged untuk JSON
        private object GetBoardJagged(GameController game)
        {
            var board = game.GetBoard();
            var jagged = new object[board.Size][];
            for (int r = 0; r < board.Size; r++)
            {
                jagged[r] = new object[board.Size];
                for (int c = 0; c < board.Size; c++)
                {
                    var cell = board.Cells[r, c];
                    jagged[r][c] = new
                    {
                        position = new { Row = cell.Position.Row, Col = cell.Position.Col },
                        piece = cell.Piece != null
                            ? new PieceDto { Color = cell.Piece.Color.ToString() }
                            : null
                    };
                }
            }
            return new { Size = board.Size, Cells = jagged };
        }

        // Helper: ambil semua valid moves untuk current player (untuk UI)
        private List<object> GetValidMoves(GameController game)
        {
            var moves = new List<object>();
            var color = game.CurrentPlayer.Color;

            for (int r = 0; r < game.GetBoard().Size; r++)
            {
                for (int c = 0; c < game.GetBoard().Size; c++)
                {
                    var pos = new Position(r, c);
                    if (game.IsValidMove(pos, color))
                        moves.Add(new { Row = r, Col = c });
                }
            }

            return moves;
        }
    }
}
