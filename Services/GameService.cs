using OthelloAPI.Models;
using OthelloAPI.Common;
using OthelloAPI.DTOs.Response;

namespace OthelloAPI.Services
{
    public class GameController
    {
        private IBoard _board;
        private List<IPlayer> _players;

        private int _currentPlayerIndex = 0;
        private int _counterPasses = 0;

        public bool IsGameOver { get; private set; } = false;
        public IPlayer CurrentPlayer => _players[_currentPlayerIndex];

        // Events untuk UI / API
        public event Action<IPlayer>? TurnChanged;
        public event Action<IBoard>? BoardUpdated;
        public event Action<IPlayer?>? GameEnded;

        private readonly Position[] directions = new Position[]
        {
            new Position(-1,0), new Position(-1,1), new Position(0,1), new Position(1,1),
            new Position(1,0), new Position(1,-1), new Position(0,-1), new Position(-1,-1)
        };

        // Constructor
        public GameController()
        {
            _players = new List<IPlayer>();
            _board = new Board(8);
        }


        // ------------------ BOARD INITIALIZATION ------------------

        public void StartNewGame(List<IPlayer> players, IBoard board)
            {
                _players = players;
                _board = board;
                _currentPlayerIndex = 0;
                _counterPasses = 0;
                IsGameOver = false;

                InitializeBoardCells();
                InitializeBoard();

                RaiseBoardUpdated();
                RaiseTurnChanged();
            }

        private void InitializeBoardCells()
        {
            for (int r = 0; r < _board.Size; r++)
                for (int c = 0; c < _board.Size; c++)
                    _board.Cells[r, c] = new Cell(new Position(r, c));
        }

        private void InitializeBoard()
        {
            int mid = _board.Size / 2;
            _board.Cells[mid - 1, mid - 1].Piece = new Piece(PieceColor.White);
            _board.Cells[mid, mid].Piece = new Piece(PieceColor.White);
            _board.Cells[mid - 1, mid].Piece = new Piece(PieceColor.Black);
            _board.Cells[mid, mid - 1].Piece = new Piece(PieceColor.Black);

            RaiseBoardUpdated();
        }

        public IBoard GetBoard() => _board;

        private PieceColor ToPieceColor(PlayerColor playerColor)
            => playerColor == PlayerColor.Black ? PieceColor.Black : PieceColor.White;

        // ------------------ GAMEPLAY ------------------
        public ServiceResult<bool> PlayAt(Position pos)
        {
            if (IsGameOver)
                return ServiceResult<bool>.Fail("Game is already over.");

            if (!IsValidMove(pos, CurrentPlayer.Color))
                return ServiceResult<bool>.Fail("Invalid move at this position.");

            // Hitung posisi lawan yang akan dibalik
            var toFlip = GetFlippablePositions(pos, CurrentPlayer.Color);

            // Tempatkan piece
            _board.Cells[pos.Row, pos.Col].Piece = new Piece(ToPieceColor(CurrentPlayer.Color));

            // Flip lawan
            FlipPieces(toFlip);

            // Reset counter pass karena ada move valid
            _counterPasses = 0;

            // Update board
            RaiseBoardUpdated();

            // Ganti turn
            SwitchTurn();

            // Skip otomatis jika pemain baru tidak punya move
            if (!HasAnyValidMove(CurrentPlayer.Color))
            {
                _counterPasses++;
                SwitchTurn();
            }

            // Cek game over
            if (CheckGameOver())
            {
                IsGameOver = true;
                RaiseGameEnded(GetWinner());
            }

            // Move berhasil
            return ServiceResult<bool>.Ok(true);
        }

        public void PassTurn()
        {
            _counterPasses++;
            SwitchTurn();

            if (!HasAnyValidMove(CurrentPlayer.Color))
            {
                _counterPasses++;
                SwitchTurn();
            }

            if (CheckGameOver())
            {
                IsGameOver = true;
                RaiseGameEnded(GetWinner());
            }
        }

        private void FlipPieces(List<Position> positions)
        {
            foreach (var p in positions)
            {
                var cell = _board.Cells[p.Row, p.Col];
                if (cell.Piece != null)
                    cell.Piece.Color = cell.Piece.Color == PieceColor.Black
                        ? PieceColor.White
                        : PieceColor.Black;
            }
        }

        public bool IsValidMove(Position pos, PlayerColor color)
        {
            if (_board.Cells[pos.Row, pos.Col].Piece != null) return false;
            return GetFlippablePositions(pos, color).Count > 0;
        }

        private List<Position> GetFlippablePositions(Position pos, PlayerColor color)
        {
            var flippable = new List<Position>();
            var myColor = ToPieceColor(color);

            foreach (var dir in directions)
            {
                var temp = new List<Position>();
                int r = pos.Row + dir.Row;
                int c = pos.Col + dir.Col;

                while (r >= 0 && r < _board.Size && c >= 0 && c < _board.Size)
                {
                    var piece = _board.Cells[r, c].Piece;
                    if (piece == null || piece.Color == PieceColor.Empty)
                    {
                        temp.Clear();
                        break;
                    }
                    else if (piece.Color == myColor)
                    {
                        if (temp.Count > 0)
                            flippable.AddRange(temp);
                        break;
                    }
                    else
                    {
                        temp.Add(new Position(r, c));
                    }

                    r += dir.Row;
                    c += dir.Col;
                }
            }

            return flippable;
        }

        private void SwitchTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            RaiseTurnChanged();
            RaiseBoardUpdated();
        }

        private bool HasAnyValidMove(PlayerColor color)
        {
            for (int r = 0; r < _board.Size; r++)
            for (int c = 0; c < _board.Size; c++)
                if (IsValidMove(new Position(r, c), color))
                    return true;

            return false;
        }

        private bool CheckGameOver()
        {
            // Board penuh
            if (_board.Cells.Cast<Cell>().All(c => c.Piece != null))
                return true;

            // Semua pemain tidak punya move
            if (!HasAnyValidMove(PlayerColor.Black) && !HasAnyValidMove(PlayerColor.White))
                return true;

            // Semua pemain pass berturut-turut
            if (_counterPasses >= _players.Count)
                return true;

            return false;
        }

        public ServiceResult<ScoreDto>  GetScore()
        {
            if (_board == null || _board.Cells == null)
                return ServiceResult<ScoreDto>.Fail("Board belum diinisialisasi");

            var score = new ScoreDto();

            foreach (var cell in _board.Cells)
            {
                if (cell.Piece == null) continue;

                if (cell.Piece.Color == PieceColor.Black) score.Black++;
                else if (cell.Piece.Color == PieceColor.White) score.White++;
            }

            return ServiceResult<ScoreDto>.Ok(score);
        }

        public IPlayer? GetWinner()
        {
            var result = GetScore();

            if (!result.Success)
            {

                return null;
            }

            var score = result.Data; // inilah ScoreDto
            if (score.Black > score.White) return _players.First(p => p.Color == PlayerColor.Black);
            if (score.White > score.Black) return _players.First(p => p.Color == PlayerColor.White);
            return null; 
        }

        // ------------------ EVENT RAISERS ------------------
        private void RaiseTurnChanged() => TurnChanged?.Invoke(CurrentPlayer);
        private void RaiseBoardUpdated() => BoardUpdated?.Invoke(_board);
        private void RaiseGameEnded(IPlayer? winner) => GameEnded?.Invoke(winner);
    }
}
