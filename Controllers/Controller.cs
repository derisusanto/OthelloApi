
    using Microsoft.AspNetCore.Mvc;
    using OthelloAPI.Models;
    using OthelloAPI.Services;
    using OthelloAPI.DTOs.Request;
    using OthelloAPI.DTOs.Response;

    namespace OthelloAPI.Controllers
    {
        [ApiController]
        [Route("[controller]")]
        public class OthelloController : ControllerBase
        {
            private readonly GameController? _game;

             public OthelloController(GameController game)
            {
                _game = game;
            }

            [HttpPost("start")]
            public IActionResult StartGame([FromBody] StartGameRequestDto request)
            {
                if (string.IsNullOrWhiteSpace(request.Player1) ||
                    string.IsNullOrWhiteSpace(request.Player2))
                {
                    Console.WriteLine($"Player1={request.Player1}, Player2={request.Player2}");
                    return BadRequest("Nama pemain wajib diisi");
                }


                var players = new List<IPlayer>
                {
                    new Player(request.Player1, PlayerColor.Black),
                    new Player(request.Player2, PlayerColor.White)
                };

                IBoard board = new Board(8);
                _game.StartNewGame(players, board);


                var scoreResult = _game.GetScore();

                var response = new GameStateDto
                {   
                    Board = GetBoardJagged(),
                    Score = scoreResult.Data,
                    CurrentPlayer = new PlayerDto
                    {
                        Name = _game.CurrentPlayer.Name,
                        Color = _game.CurrentPlayer.Color.ToString()
                    },
                     Players = new List<PlayerDto>
    {
        new PlayerDto
        {
            Name = players[0].Name,
            Color = players[0].Color.ToString()
        },
        new PlayerDto
        {
            Name = players[1].Name,
            Color = players[1].Color.ToString()
        }
    },
                    ValidMoves = GetValidMoves(),
                    IsGameOver = _game.IsGameOver
                    
                };


            return Ok(response);
}

            [HttpGet("board")]
            public IActionResult GetBoard()
            {
                if (_game == null) return BadRequest("Game belum dimulai");

                var board = _game.GetBoard(); // Board selalu bukan null
                // var boardDto = new BoardDto
                // {
                //     Size = board.Size,
                //     Cells = new List<List<CellDto>>()
                // };
                
        
                // for (int r = 0; r < board.Size; r++)
                // {
                //     var row = new List<CellDto>();
                //     for (int c = 0; c < board.Size; c++)
                //     {
                //         var cell = board.Cells[r, c];
                //         row.Add(new CellDto
                //         {
                //             Position = new PositionDto { Row = cell.Position.Row, Col = cell.Position.Col },
                //             Piece = cell.Piece != null ? new PieceDto { Color = cell.Piece.Color.ToString() } : null
                //         });
                //     }
                //     boardDto.Cells.Add(row);
                // }
                var boardDto = GetBoardJagged();

                return Ok(boardDto);
            }


            [HttpPost("play")]
            public IActionResult Play([FromBody] PlayMoveRequestDto request)
            {
                if (_game == null) 
                    return BadRequest("Game belum dimulai");

                Position pos = new Position(request.Row, request.Col);

                // Gunakan ServiceResult versi baru
                var playResult = _game.PlayAt(pos);

                if (!playResult.Success) 
                    return BadRequest(playResult.Message);

                var scoreResult = _game.GetScore(); // ServiceResult<ScoreDto>

                if (!scoreResult.Success)
                    return BadRequest(scoreResult.Message);

                var response = new GameStateDto
                {
                    Score = scoreResult.Data, // ambil dari Data
                    CurrentPlayer = new PlayerDto
                    {
                        Name = _game.CurrentPlayer.Name,
                        Color = _game.CurrentPlayer.Color.ToString()
                    },
                    ValidMoves = GetValidMoves(),
                    IsGameOver = _game.IsGameOver
                };

                return Ok(response);
            }


            [HttpPost("pass")]
            public IActionResult Pass()
            {
                if (_game == null) return BadRequest("Game belum dimulai");

                _game.PassTurn();

            var scoreResult = _game.GetScore(); // ServiceResult<ScoreDto>

                if (!scoreResult.Success)
                    return BadRequest(scoreResult.Message);

                var response = new GameStateDto
                {
                    Score = scoreResult.Data, // ambil dari Data
                    CurrentPlayer = new PlayerDto
                    {
                        Name = _game.CurrentPlayer.Name,
                        Color = _game.CurrentPlayer.Color.ToString()
                    },
                    ValidMoves = GetValidMoves(),
                    IsGameOver = _game.IsGameOver
                };

                return Ok(response);
            }

            [HttpGet("status")]
            public IActionResult Status()
            {
                if (_game == null) return BadRequest("Game belum dimulai");

                  var scoreResult = _game.GetScore();

                var response = new GameStateDto
                {
                    Score = scoreResult.Data, // ambil dari Data

                    CurrentPlayer = new PlayerDto
                    {
                        Name = _game.CurrentPlayer.Name,
                        Color = _game.CurrentPlayer.Color.ToString()
                    },
                    
                    ValidMoves = GetValidMoves(),
                    Board = GetBoardJagged(),
                    IsGameOver = _game.IsGameOver
                };

                return Ok(response);
            }

            // Helper: convert Player ke DTO
            // private PlayerDto ToDto(IPlayer p)
            //     => new PlayerDto { Name = p.Name, Color = p.Color.ToString() };

            // Helper: buat board jagged untuk JSON
        private BoardDto GetBoardJagged()
        {
            var board = _game.GetBoard();

            var dto = new BoardDto
            {
                Size = board.Size,
                Cells = new List<List<CellDto>>()
            };

            for (int r = 0; r < board.Size; r++)
            {
                var row = new List<CellDto>();

                for (int c = 0; c < board.Size; c++)
                {
                    var cell = board.Cells[r, c];

                    row.Add(new CellDto
                    {
                        Position = new PositionDto
                        {
                            Row = cell.Position.Row,
                            Col = cell.Position.Col
                        },
                        Piece = cell.Piece != null
                            ? new PieceDto
                            {
                                Color = cell.Piece.Color.ToString()
                            }
                            : null
                    });
                }

                dto.Cells.Add(row);
            }

            return dto;
        }

            // Helper: ambil semua valid moves untuk current player (untuk UI)
            private List<PositionDto> GetValidMoves()
            {
                var moves = new List<PositionDto>();
                var color = _game.CurrentPlayer.Color;

                for (int r = 0; r < _game.GetBoard().Size; r++)
                {
                    for (int c = 0; c < _game.GetBoard().Size; c++)
                    {
                        var pos = new Position(r, c);
                        if (_game.IsValidMove(pos, color))
                            moves.Add(new PositionDto{ Row = r, Col = c });
                    }
                }

                return moves;
            }
        }
    }