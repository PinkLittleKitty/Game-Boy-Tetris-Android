using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set;}
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    private int lastTetromino;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public int level = 0;
    public TextMeshProUGUI levelText;
    public int lines = 0;
    private int levelLines = 0;
    public TextMeshProUGUI linesText;
    public int score = 0;
    public TextMeshProUGUI scoreText;

    public GameObject gameOverPanel;
    public bool isGameOver;

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
        QualitySettings.vSyncCount = 1;
	    Application.targetFrameRate = 30;

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

    private void Update()
    {
        if (isGameOver)
        {
            if (Input.anyKeyDown)
            {
                Restart();
            }
        }
        else
        {
            levelText.text = this.level.ToString();
            linesText.text = this.lines.ToString();
            scoreText.text = this.score.ToString();
        }
    }

    public void SpawnPiece()
    {
        if (isGameOver) return;

        TetrominoData data = this.tetrominoes[RandomizeTetromino()];

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        } else {
            GameOver();
        }
    }

    private int RandomizeTetromino()
    {
        int random1 = Random.Range(0, this.tetrominoes.Length);
        if (random1 == lastTetromino)
        {
            int random2 = Random.Range(0, this.tetrominoes.Length);
            lastTetromino = random2;
            return random2;
        }

        lastTetromino = random1;
        return random1;
    }

    private void GameOver()
    {
        isGameOver = true;
        this.tilemap.ClearAllTiles();
        this.gameObject.GetComponent<Piece>().enabled = false;
        gameOverPanel.SetActive(true);
    }

    private void Restart()
    {
        this.gameObject.GetComponent<Piece>().enabled = true;
        isGameOver = false;
        gameOverPanel.SetActive(false);
        lines = 0;
        levelLines = 0;
        level = 0;
        score = 0;
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
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            if (tilemap.HasTile(tilePosition)) {
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

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                lines++;
                levelLines++;
                score += 100;
                CheckLevelLines();
            } else {
                row++;
            }
        }
    }

    private void CheckLevelLines()
    {
        if (levelLines == 10)
        {
            level++;
            AudioManager.instance.PlaySfx(GlobalSfx.LevelUp);
            ColourChanger.instance.ChangeColour(Random.Range(0, ColourChanger.instance.colorPalettes.Length));
            if (activePiece.stepDelay > 0.1f)
            {
                activePiece.stepDelay = activePiece.stepDelay - 0.1f;
            }
            levelLines = 0;
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if(!this.tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
