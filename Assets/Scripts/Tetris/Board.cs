using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set;}
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    private readonly List<int> pieceBag = new List<int>();
    public Vector3Int spawnPosition;
    
    [Header("Piece Preview")]
    public NextPieceDisplay nextPieceDisplay;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public GameObject pausePanel;
    public bool isPaused { get; private set; }
    private GameObject autoPauseObject;
    private bool isCurtainRunning;

    public TetrominoData? heldPiece { get; private set; }
    public bool canHold { get; private set; } = true;

    public int level = 0;
    public TextMeshProUGUI levelText;
    public int lines = 0;
    private int levelLines = 0;
    public TextMeshProUGUI linesText;
    public int score = 0;
    public int baseScorePerLine = 100;

    public TextMeshProUGUI scoreText;

    public GameObject gameOverPanel;
    public bool isGameOver;

    private Leaderboard leaderboard;

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
        leaderboard = GetComponent<Leaderboard>();
        this.level = 0;
        SpawnPiece();
    }

    private void Update()
    {
        if (playerInput != null && playerInput.Movement.Start.WasPressedThisFrame())
        {
            if (!isGameOver && !isCurtainRunning)
            {
                TogglePause();
            }
        }

        if (isGameOver)
        {
            if (Input.anyKeyDown && !isCurtainRunning)
            {
                Restart();
            }
            return;
        }

        if (isPaused) return;

        levelText.text = this.level.ToString();
        linesText.text = this.lines.ToString();
        scoreText.text = this.score.ToString();
    }

    private Coroutine pauseBlinkCoroutine;

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        else
        {
            EnsureAutoPauseObject();
            if (autoPauseObject != null)
            {
                autoPauseObject.SetActive(isPaused);
                if (isPaused)
                {
                    if (pauseBlinkCoroutine != null) StopCoroutine(pauseBlinkCoroutine);
                    pauseBlinkCoroutine = StartCoroutine(PauseBlinkRoutine());
                }
                else
                {
                    if (pauseBlinkCoroutine != null)
                    {
                        StopCoroutine(pauseBlinkCoroutine);
                        pauseBlinkCoroutine = null;
                    }
                }
            }
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
        }
    }

    private IEnumerator PauseBlinkRoutine()
    {
        TextMeshPro tmp = autoPauseObject != null ? autoPauseObject.GetComponentInChildren<TextMeshPro>() : null;
        if (tmp == null) yield break;

        while (isPaused)
        {
            tmp.enabled = !tmp.enabled;
            yield return new WaitForSecondsRealtime(0.4f);
        }
        tmp.enabled = true;
    }

    private void EnsureAutoPauseObject()
    {
        if (autoPauseObject == null)
        {
            autoPauseObject = new GameObject("PauseOverlay");
            autoPauseObject.transform.SetParent(this.transform, false);
            autoPauseObject.transform.localPosition = new Vector3(Bounds.center.x, Bounds.center.y, 0);

            GameObject bg = new GameObject("PauseBG");
            bg.transform.SetParent(autoPauseObject.transform, false);
            bg.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.6f, 0.76f, 0.16f, 0.95f));
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(7f, 2.5f);
            sr.sortingOrder = 50;

            GameObject textObj = new GameObject("PauseText");
            textObj.transform.SetParent(autoPauseObject.transform, false);
            textObj.transform.localPosition = Vector3.zero;
            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            
            if (levelText != null && levelText.font != null)
            {
                tmp.font = levelText.font;
            }
            tmp.text = "PAUSE";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.04f, 0.2f, 0.04f, 1f);

            MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 51;

            autoPauseObject.SetActive(false);
        }
    }

    public float GetStepDelayForLevel(int currentLevel)
    {
        float delay = Mathf.Max(0.08f, 0.85f - (currentLevel * 0.08f));
        return delay;
    }

    public void SpawnPiece()
    {
        if (isGameOver || isCurtainRunning) return;
        if (this.tetrominoes == null || this.tetrominoes.Length == 0)
        {
            Debug.LogWarning("Board: tetromino list is empty; cannot spawn piece.", this);
            return;
        }

        if (this.activePiece == null)
        {
            this.activePiece = GetComponentInChildren<Piece>();
            if (this.activePiece == null)
            {
                Debug.LogError("Board: activePiece component missing; cannot spawn piece.", this);
                return;
            }
        }

        TetrominoData data;
        
        if (nextPieceDisplay != null)
        {
            nextPieceDisplay.EnsureNextPiece();
            data = nextPieceDisplay.GetNextPiece();
        }
        else
        {
            data = this.tetrominoes[RandomizeTetromino()];
        }

        this.activePiece.enabled = true;
        this.activePiece.Initialize(this, this.spawnPosition, data);
        this.activePiece.stepDelay = GetStepDelayForLevel(this.level);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        } else {
            GameOver();
        }

        canHold = true;
    }

    public int RandomizeTetromino()
    {
        if (this.tetrominoes == null || this.tetrominoes.Length == 0) return 0;

        if (pieceBag.Count == 0)
        {
            RefillBag();
        }

        int index = pieceBag[0];
        pieceBag.RemoveAt(0);
        return index;
    }

    private void RefillBag()
    {
        List<int> newBag = new List<int>();
        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            newBag.Add(i);
        }

        for (int i = newBag.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            int temp = newBag[i];
            newBag[i] = newBag[randIndex];
            newBag[randIndex] = temp;
        }

        pieceBag.AddRange(newBag);
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        leaderboard.UploadScore(this.score);

        StartCoroutine(GameOverCurtainRoutine());
    }

    private IEnumerator GameOverCurtainRoutine()
    {
        isCurtainRunning = true;
        if (activePiece != null) activePiece.enabled = false;

        Tile curtainTile = (tetrominoes != null && tetrominoes.Length > 0) ? tetrominoes[0].tile : null;
        RectInt bounds = this.Bounds;

        for (int row = bounds.yMin; row < bounds.yMax; row++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int pos = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(pos, curtainTile);
            }
            yield return new WaitForSeconds(0.025f);
        }

        yield return new WaitForSeconds(0.15f);

        for (int row = bounds.yMax - 1; row >= bounds.yMin; row--)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int pos = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(pos, null);
            }
            yield return new WaitForSeconds(0.025f);
        }

        this.tilemap.ClearAllTiles();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        leaderboard.UploadScore(score);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(GlobalSfx.GameOver);
        }

        isCurtainRunning = false;
    }

    private void Restart()
    {
        if (isCurtainRunning) return;
        StopAllCoroutines();
        this.tilemap.ClearAllTiles();
        if (this.activePiece != null) this.activePiece.enabled = true;
        isGameOver = false;
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (autoPauseObject != null) autoPauseObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        lines = 0;
        levelLines = 0;
        level = 0;
        score = 0;
        heldPiece = null;
        canHold = true;
        pieceBag.Clear();
        SpawnPiece();
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

    public void ClearLines(bool isTSpin = false)
    {
        RectInt bounds = this.Bounds;
        List<int> fullRows = new List<int>();

        for (int row = bounds.yMin; row < bounds.yMax; row++)
        {
            if (IsLineFull(row))
            {
                fullRows.Add(row);
            }
        }

        if (fullRows.Count > 0)
        {
            StartCoroutine(LineClearRoutine(fullRows, isTSpin));
        }
        else
        {
            if (isTSpin)
            {
                int multiplier = level + 1;
                score += 400 * multiplier;
            }

            SpawnPiece();
        }
    }

    private IEnumerator LineClearRoutine(List<int> fullRows, bool isTSpin)
    {
        RectInt bounds = this.Bounds;

        Dictionary<Vector3Int, TileBase> savedTiles = new Dictionary<Vector3Int, TileBase>();
        foreach (int row in fullRows)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int pos = new Vector3Int(col, row, 0);
                savedTiles[pos] = this.tilemap.GetTile(pos);
            }
        }

        for (int flash = 0; flash < 3; flash++)
        {
            foreach (var kvp in savedTiles)
            {
                this.tilemap.SetTile(kvp.Key, null);
            }
            yield return new WaitForSeconds(0.045f);

            foreach (var kvp in savedTiles)
            {
                this.tilemap.SetTile(kvp.Key, kvp.Value);
            }
            yield return new WaitForSeconds(0.045f);
        }

        foreach (var kvp in savedTiles)
        {
            this.tilemap.SetTile(kvp.Key, null);
        }

        fullRows.Sort((a, b) => b.CompareTo(a));
        foreach (int row in fullRows)
        {
            LineClear(row);
        }

        int clearedLines = fullRows.Count;
        lines += clearedLines;
        levelLines += clearedLines;

        int multiplier = level + 1;

        if (isTSpin)
        {
            switch (clearedLines)
            {
                case 1:
                    score += 800 * multiplier;
                    break;
                case 2:
                    score += 1200 * multiplier;
                    break;
                case 3:
                    score += 1600 * multiplier;
                    break;
                default:
                    score += 400 * multiplier;
                    break;
            }
        }
        else
        {
            int scorePerLine = baseScorePerLine * multiplier;
            switch (clearedLines)
            {
                case 1:
                    score += scorePerLine * 1;
                    break;
                case 2:
                    score += scorePerLine * 3;
                    break;
                case 3:
                    score += scorePerLine * 5;
                    break;
                case 4:
                    score += scorePerLine * 8;
                    break;
            }
        }

        CheckLevelLines();
        SpawnPiece();
    }


    private void CheckLevelLines()
    {
        if (levelLines >= 10)
        {
            level++;
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(GlobalSfx.LevelUp);
            }
            if (ColourChanger.instance != null && ColourChanger.instance.colorPalettes != null && ColourChanger.instance.colorPalettes.Length > 0)
            {
                ColourChanger.instance.ChangeColour(Random.Range(0, ColourChanger.instance.colorPalettes.Length));
            }
            if (activePiece != null)
            {
                activePiece.stepDelay = GetStepDelayForLevel(this.level);
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

    public void HoldPiece()
    {
        if (!canHold) return;
        Clear(activePiece);

        if (heldPiece.HasValue)
        {
            TetrominoData currentData = activePiece.data;
            TetrominoData heldData = heldPiece.Value;
            
            heldPiece = currentData;
            
            activePiece.Initialize(this, spawnPosition, heldData);
            
            if (IsValidPosition(activePiece, spawnPosition))
            {
                Set(activePiece);
            }
            else
            {
                GameOver();
                return;
            }
        }
        else
        {
            heldPiece = activePiece.data;
            SpawnPiece();
            return;
        }

        canHold = false;
        
        AudioManager.instance.PlaySfx(GlobalSfx.Rotate);
    }
}
