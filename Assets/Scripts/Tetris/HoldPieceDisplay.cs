using UnityEngine;
using UnityEngine.Tilemaps;

public class HoldPieceDisplay : MonoBehaviour
{
    [Header("Hold Display Settings")]
    public Tilemap holdTilemap;
    public Board board;
    public Vector3Int displayPosition = Vector3Int.zero;
    
    private TetrominoData? lastDisplayedPiece;

    private void Update()
    {
        UpdateHoldDisplay();
    }

    private void UpdateHoldDisplay()
    {
        if (!AreTetrominoDataEqual(board.heldPiece, lastDisplayedPiece))
        {
            ClearDisplay();
            
            if (board.heldPiece.HasValue)
            {
                DisplayPiece(board.heldPiece.Value);
            }
            
            lastDisplayedPiece = board.heldPiece;
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
        Vector2Int[] cells = data.cells;
        
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = displayPosition + (Vector3Int)cells[i];
            holdTilemap.SetTile(tilePosition, data.tile);
        }
    }

    private void ClearDisplay()
    {
        if (lastDisplayedPiece.HasValue)
        {
            Vector2Int[] cells = lastDisplayedPiece.Value.cells;
            
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3Int tilePosition = displayPosition + (Vector3Int)cells[i];
                holdTilemap.SetTile(tilePosition, null);
            }
        }
    }
}