using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set;}
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int((-this.boardSize.x / 2) - 3, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    public Controls playerInput;

    private void Awake()
    {
        this.tilemap = this.GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        playerInput = new Controls();
        playerInput.Enable();

        for (int i = 0; i < this.tetrominoes.Length; i++) 
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);
        Set(this.activePiece);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        DrawRect(this.Bounds);
    }

    void DrawRect(RectInt rect)
    {
        Gizmos.DrawWireCube(new Vector3(rect.center.x, rect.center.y, 0.01f), new Vector3(rect.size.x, rect.size.y, 0.01f));
    }
}
