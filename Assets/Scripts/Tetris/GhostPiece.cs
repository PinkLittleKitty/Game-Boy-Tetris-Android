using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostPiece : MonoBehaviour
{
    [Header("References")]
    public Board board;
    public Piece activePiece;
    public Tilemap ghostTilemap;
    public Tile ghostTile;

    public Vector3Int dropPosition { get; private set; }
    private readonly List<Vector3Int> previousPositions = new List<Vector3Int>();

    private void LateUpdate()
    {
        if (board == null || board.isGameOver || activePiece == null || !activePiece.enabled)
        {
            Clear();
            return;
        }

        Clear();
        CalculateDropPosition();
        Draw();
    }

    private void CalculateDropPosition()
    {
        if (activePiece == null || activePiece.cells == null || board == null) return;

        dropPosition = activePiece.position;
        Vector3Int testPosition = dropPosition;

        board.Clear(activePiece);

        while (board.IsValidPosition(activePiece, testPosition + Vector3Int.down))
        {
            testPosition += Vector3Int.down;
        }

        dropPosition = testPosition;

        board.Set(activePiece);
    }

    public void Draw()
    {
        if (ghostTilemap == null || activePiece == null || activePiece.cells == null) return;

        Tile tileToDraw = ghostTile != null ? ghostTile : activePiece.data.tile;
        if (tileToDraw == null) return;

        for (int i = 0; i < activePiece.cells.Length; i++)
        {
            Vector3Int tilePosition = activePiece.cells[i] + dropPosition;
            ghostTilemap.SetTile(tilePosition, tileToDraw);
            previousPositions.Add(tilePosition);
        }
    }

    public void Clear()
    {
        if (ghostTilemap == null) return;

        if (ghostTilemap != board?.tilemap)
        {
            ghostTilemap.ClearAllTiles();
            previousPositions.Clear();
        }
        else
        {
            for (int i = 0; i < previousPositions.Count; i++)
            {
                ghostTilemap.SetTile(previousPositions[i], null);
            }
            previousPositions.Clear();
        }
    }
}
