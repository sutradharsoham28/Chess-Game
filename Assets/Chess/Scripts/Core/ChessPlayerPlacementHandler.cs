using UnityEngine;
using System.Collections.Generic;

namespace Chess.Scripts.Core
{
    public enum PieceType
    {
        Pawn, Rook, Knight, Bishop, Queen, King
    }

    public enum PieceColor
    {
        White, Black
    }

    [System.Serializable]
    public struct Move
    {
        public int row;
        public int col;
        public bool isCapture;

        public Move(int row, int col, bool isCapture = false)
        {
            this.row = row;
            this.col = col;
            this.isCapture = isCapture;
        }
    }

    public class ChessPlayerPlacementHandler : MonoBehaviour
    {
        [Header("Piece Settings")]
        [SerializeField] public int row, column;
        [SerializeField] public PieceType pieceType;
        [SerializeField] public PieceColor pieceColor;

        [Header("Highlight Prefabs")]
        [SerializeField] private GameObject normalMovePrefab;
        [SerializeField] private GameObject captureMovePrefab;

        // Position tracking for runtime updates
        private int oldRow;
        private int oldColumn;

        // Selection state
        private bool isSelected = false;
        private static ChessPlayerPlacementHandler selectedPiece;

        // Board state - shared among all pieces
        private static ChessPlayerPlacementHandler[,] board = new ChessPlayerPlacementHandler[8, 8];
        private static bool boardInitialized = false;

        private void Start()
        {
            InitializeBoard();
            MovePiece();
            UpdateBoardPosition();

            oldRow = row;
            oldColumn = column;
        }

        private void Update()
        {
            // Handle position changes during runtime
            if (row != oldRow || column != oldColumn)
            {
                ClearFromBoard();
                MovePiece();
                UpdateBoardPosition();

                oldRow = row;
                oldColumn = column;
            }
        }

        private void OnMouseDown()
        {
            SelectPiece();
        }

        #region Piece Movement and Positioning

        private void MovePiece()
        {
            var tile = ChessBoardPlacementHandler.Instance.GetTile(row, column);
            if (tile != null)
            {
                transform.position = tile.transform.position;
            }
        }

        private void InitializeBoard()
        {
            if (!boardInitialized)
            {
                // Clear the board
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        board[i, j] = null;
                    }
                }

                // Find all pieces and add them to board
                var allPieces = FindObjectsOfType<ChessPlayerPlacementHandler>();
                foreach (var piece in allPieces)
                {
                    if (IsValidPosition(piece.row, piece.column))
                    {
                        board[piece.row, piece.column] = piece;
                    }
                }

                boardInitialized = true;
            }
        }

        private void UpdateBoardPosition()
        {
            if (IsValidPosition(row, column))
            {
                board[row, column] = this;
            }
        }

        private void ClearFromBoard()
        {
            if (IsValidPosition(oldRow, oldColumn))
            {
                board[oldRow, oldColumn] = null;
            }
        }

        #endregion

        #region Piece Selection and Highlighting

        private void SelectPiece()
        {
            // If clicking the same piece, deselect it
            if (selectedPiece == this)
            {
                DeselectPiece();
                return;
            }

            // Deselect previous piece
            if (selectedPiece != null)
            {
                selectedPiece.DeselectPiece();
            }

            // Select this piece
            selectedPiece = this;
            isSelected = true;
            HighlightPossibleMoves();
        }

        private void DeselectPiece()
        {
            isSelected = false;
            selectedPiece = null;
            ClearAllHighlights();
        }

        private void HighlightPossibleMoves()
        {
            var possibleMoves = GetPossibleMoves();
            foreach (var move in possibleMoves)
            {
                HighlightMove(move.row, move.col, move.isCapture);
            }
        }

        private void HighlightMove(int row, int col, bool isCapture)
        {
            var prefabToUse = isCapture ? captureMovePrefab : normalMovePrefab;
            if (prefabToUse != null && ChessBoardPlacementHandler.Instance != null)
            {
                var tile = ChessBoardPlacementHandler.Instance.GetTile(row, col);
                if (tile != null)
                {
                    Instantiate(prefabToUse, tile.transform.position, Quaternion.identity, tile.transform);
                }
            }
        }

        private void ClearAllHighlights()
        {
            if (ChessBoardPlacementHandler.Instance != null)
            {
                ChessBoardPlacementHandler.Instance.ClearHighlights();
            }
        }

        #endregion

        #region Move Calculation - All Pieces in One Method

        public List<Move> GetPossibleMoves()
        {
            List<Move> moves = new List<Move>();

            switch (pieceType)
            {
                case PieceType.Pawn:
                    moves = GetPawnMoves();
                    break;
                case PieceType.Rook:
                    moves = GetRookMoves();
                    break;
                case PieceType.Knight:
                    moves = GetKnightMoves();
                    break;
                case PieceType.Bishop:
                    moves = GetBishopMoves();
                    break;
                case PieceType.Queen:
                    moves = GetQueenMoves();
                    break;
                case PieceType.King:
                    moves = GetKingMoves();
                    break;
            }

            return moves;
        }

        private List<Move> GetPawnMoves()
        {
            List<Move> moves = new List<Move>();
            int direction = (pieceColor == PieceColor.White) ? -1 : 1;

            int newRow = row + direction;

            // Forward move (1 square)
            if (IsValidPosition(newRow, column) && GetPieceAt(newRow, column) == null)
            {
                moves.Add(new Move(newRow, column));

                // Forward move (2 squares from starting position)
                int startingRow = (pieceColor == PieceColor.White) ? 1 : 6;
                if (row == startingRow)
                {
                    int twoStepRow = row + (2 * direction);
                    if (IsValidPosition(twoStepRow, column) && GetPieceAt(twoStepRow, column) == null)
                    {
                        moves.Add(new Move(twoStepRow, column));
                    }
                }
            }


            // Diagonal captures
            int[] captureColumns = { column - 1, column + 1 };
            foreach (int captureCol in captureColumns)
            {
                newRow = row + direction;
                if (IsValidPosition(newRow, captureCol) && HasEnemyPiece(newRow, captureCol))
                {
                    moves.Add(new Move(newRow, captureCol, true));
                }
            }

            return moves;
        }

        private List<Move> GetRookMoves()
        {
            List<Move> moves = new List<Move>();

            // Horizontal and vertical directions
            int[,] directions = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int deltaRow = directions[d, 0];
                int deltaCol = directions[d, 1];

                for (int distance = 1; distance < 8; distance++)
                {
                    int newRow = row + (deltaRow * distance);
                    int newCol = column + (deltaCol * distance);

                    if (!IsValidPosition(newRow, newCol)) break;

                    var pieceAtPosition = GetPieceAt(newRow, newCol);
                    if (pieceAtPosition == null)
                    {
                        moves.Add(new Move(newRow, newCol));
                    }
                    else if (pieceAtPosition.pieceColor != this.pieceColor)
                    {
                        moves.Add(new Move(newRow, newCol, true));
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }

        private List<Move> GetKnightMoves()
        {
            List<Move> moves = new List<Move>();

            // All possible knight moves (L-shapes)
            int[,] knightMoves = {
                {2, 1}, {2, -1}, {-2, 1}, {-2, -1},
                {1, 2}, {1, -2}, {-1, 2}, {-1, -2}
            };

            for (int i = 0; i < knightMoves.GetLength(0); i++)
            {
                int newRow = row + knightMoves[i, 0];
                int newCol = column + knightMoves[i, 1];

                if (CanMoveTo(newRow, newCol))
                {
                    bool isCapture = HasEnemyPiece(newRow, newCol);
                    moves.Add(new Move(newRow, newCol, isCapture));
                }
            }

            return moves;
        }

        private List<Move> GetBishopMoves()
        {
            List<Move> moves = new List<Move>();

            // Diagonal directions
            int[,] directions = { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int deltaRow = directions[d, 0];
                int deltaCol = directions[d, 1];

                for (int distance = 1; distance < 8; distance++)
                {
                    int newRow = row + (deltaRow * distance);
                    int newCol = column + (deltaCol * distance);

                    if (!IsValidPosition(newRow, newCol)) break;

                    var pieceAtPosition = GetPieceAt(newRow, newCol);
                    if (pieceAtPosition == null)
                    {
                        moves.Add(new Move(newRow, newCol));
                    }
                    else if (pieceAtPosition.pieceColor != this.pieceColor)
                    {
                        moves.Add(new Move(newRow, newCol, true));
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }

        private List<Move> GetQueenMoves()
        {
            List<Move> moves = new List<Move>();

            // Queen moves like both rook and bishop
            int[,] directions = {
                {0, 1}, {0, -1}, {1, 0}, {-1, 0},      // Rook moves
                {1, 1}, {1, -1}, {-1, 1}, {-1, -1}     // Bishop moves
            };

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int deltaRow = directions[d, 0];
                int deltaCol = directions[d, 1];

                for (int distance = 1; distance < 8; distance++)
                {
                    int newRow = row + (deltaRow * distance);
                    int newCol = column + (deltaCol * distance);

                    if (!IsValidPosition(newRow, newCol)) break;

                    var pieceAtPosition = GetPieceAt(newRow, newCol);
                    if (pieceAtPosition == null)
                    {
                        moves.Add(new Move(newRow, newCol));
                    }
                    else if (pieceAtPosition.pieceColor != this.pieceColor)
                    {
                        moves.Add(new Move(newRow, newCol, true));
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }

        private List<Move> GetKingMoves()
        {
            List<Move> moves = new List<Move>();

            // King can move one square in any direction
            int[,] directions = {
                {0, 1}, {0, -1}, {1, 0}, {-1, 0},
                {1, 1}, {1, -1}, {-1, 1}, {-1, -1}
            };

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int newRow = row + directions[d, 0];
                int newCol = column + directions[d, 1];

                if (CanMoveTo(newRow, newCol))
                {
                    bool isCapture = HasEnemyPiece(newRow, newCol);
                    moves.Add(new Move(newRow, newCol, isCapture));
                }
            }

            return moves;
        }

        #endregion

        #region Helper Methods

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        private ChessPlayerPlacementHandler GetPieceAt(int row, int col)
        {
            if (IsValidPosition(row, col))
            {
                return board[row, col];
            }
            return null;
        }

        private bool CanMoveTo(int row, int col)
        {
            if (!IsValidPosition(row, col)) return false;

            var pieceAtPosition = GetPieceAt(row, col);
            return pieceAtPosition == null || pieceAtPosition.pieceColor != this.pieceColor;
        }

        private bool HasEnemyPiece(int row, int col)
        {
            var pieceAtPosition = GetPieceAt(row, col);
            return pieceAtPosition != null && pieceAtPosition.pieceColor != this.pieceColor;
        }

        #endregion
    }
}