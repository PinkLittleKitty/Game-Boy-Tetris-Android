using UnityEngine;
using UnityEngine.Tilemaps;

public class NextPieceDisplay : MonoBehaviour
{
    [Header("Next Piece Display Settings")]
    public Tilemap nextTilemap;
    public Board board;
    public Vector3Int displayPosition = Vector3Int.zero;
    
    private TetrominoData? nextPiece;
    private TetrominoData? lastDisplayedPiece;

    private void Awake()
    {
        // Auto-link the board if not set in the inspector
        if (board == null)
        {
            board = GetComponentInParent<Board>();
        }
    }

    private void Start()
    {
        EnsureNextPiece();
    }

    private void Update()
    {
        UpdateNextDisplay();
    }

    public TetrominoData GetNextPiece()
    {
        EnsureNextPiece();

        if (!nextPiece.HasValue)
        {
            Debug.LogWarning("NextPieceDisplay: nextPiece is missing; returning fallback.", this);
            return GetFallbackPiece();
        }

        TetrominoData pieceToReturn = nextPiece.Value;
        GenerateNextPiece();
        return pieceToReturn;
    }

    private void GenerateNextPiece()
    {
        if (board == null || board.tetrominoes == null || board.tetrominoes.Length == 0)
        {
            Debug.LogWarning("NextPieceDisplay: Board or tetrominoes not set; cannot generate next piece.", this);
            nextPiece = null;
            return;
        }

        nextPiece = board.tetrominoes[board.RandomizeTetromino()];
    }

    public void EnsureNextPiece()
    {
        if (!nextPiece.HasValue)
        {
            GenerateNextPiece();
        }
    }

    private TetrominoData GetFallbackPiece()
    {
        if (board != null && board.tetrominoes != null && board.tetrominoes.Length > 0)
        {
            return board.tetrominoes[0];
        }

        return default;
    }

    private void UpdateNextDisplay()
    {
        if (!AreTetrominoDataEqual(nextPiece, lastDisplayedPiece))
        {
            ClearDisplay();
            
            if (nextPiece.HasValue)
            {
                DisplayPiece(nextPiece.Value);
            }
            
            lastDisplayedPiece = nextPiece;
        }
    }

    private bool AreTetrominoDataEqual(TetrominoData? a, TetrominoData? b)
    {
        if (!a.HasValue && !b.HasValue) return true;
        if (!a.HasValue || !b.HasValue) return false;
        return a.Value.tetromino == b.Value.tetromino;
    }

    private void DisplayPiece(TetrominoData data)
    {
        if (nextTilemap == null) return;

        Vector2Int[] cells = data.cells;
        
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = displayPosition + (Vector3Int)cells[i];
            nextTilemap.SetTile(tilePosition, data.tile);
        }
    }

    private void ClearDisplay()
    {
        if (nextTilemap == null) return;

        if (lastDisplayedPiece.HasValue)
        {
            Vector2Int[] cells = lastDisplayedPiece.Value.cells;
            
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3Int tilePosition = displayPosition + (Vector3Int)cells[i];
                nextTilemap.SetTile(tilePosition, null);
            }
        }
    }
}