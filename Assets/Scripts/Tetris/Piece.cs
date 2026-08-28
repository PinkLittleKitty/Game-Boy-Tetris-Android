using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    public int maxLockResets = 15;

    public float dasDelay = 0.17f;
    public float arrDelay = 0.05f;
    public float softDropDelay = 0.05f;

    private float stepTime;
    private float lockTime;
    private int lockResets;
    private bool lastActionWasRotation;

    private float horizontalMoveTimer;
    private float softDropTimer;
    private int horizontalDirection;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.enabled = true;
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.lockResets = 0;
        this.lastActionWasRotation = false;
        this.horizontalDirection = 0;

        if (this.cells == null || this.cells.Length != data.cells.Length)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        Vector2Int[] spawnCells = Data.RotationStates[data.tetromino][0];
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)spawnCells[i];
        }
    }

    private void Update()
    {
        if (this.board == null || this.board.isGameOver) return;

        this.board.Clear(this);

        HandleInput();
        if (!this.enabled) return;

        bool isGrounded = !board.IsValidPosition(this, position + Vector3Int.down);

        if (isGrounded)
        {
            this.lockTime += Time.deltaTime;
            if (this.lockTime >= this.lockDelay)
            {
                Lock();
                return;
            }
        }
        else
        {
            this.lockTime = 0f;
        }

        if (Time.time >= this.stepTime)
        {
            Step();
            if (!this.enabled) return;
        }

        this.board.Set(this);
    }

    private void HandleInput()
    {
        if (this.board.playerInput.Movement.A.WasPressedThisFrame())
        {
            Rotate(1);
        }
        else if (this.board.playerInput.Movement.B.WasPressedThisFrame())
        {
            Rotate(-1);
        }

        bool leftPressed = this.board.playerInput.Movement.Left.IsPressed();
        bool rightPressed = this.board.playerInput.Movement.Right.IsPressed();

        int currentDir = 0;
        if (leftPressed && !rightPressed) currentDir = -1;
        else if (rightPressed && !leftPressed) currentDir = 1;

        if (currentDir != 0)
        {
            if (currentDir != horizontalDirection)
            {
                horizontalDirection = currentDir;
                Move(new Vector2Int(horizontalDirection, 0));
                horizontalMoveTimer = Time.time + dasDelay;
            }
            else if (Time.time >= horizontalMoveTimer)
            {
                Move(new Vector2Int(horizontalDirection, 0));
                horizontalMoveTimer = Time.time + arrDelay;
            }
        }
        else
        {
            horizontalDirection = 0;
        }

        if (this.board.playerInput.Movement.Down.IsPressed())
        {
            if (Time.time >= softDropTimer)
            {
                if (Move(Vector2Int.down))
                {
                    this.board.score += 1;
                }
                softDropTimer = Time.time + softDropDelay;
                this.stepTime = Time.time + this.stepDelay;
            }
        }

        if (this.board.playerInput.Movement.Up.WasPressedThisFrame())
        {
            HardDrop();
            return;
        }

        if (this.board.playerInput.Movement.Select.WasPressedThisFrame())
        {
            this.board.HoldPiece();
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        if (!Move(Vector2Int.down))
        {
            if (this.lockTime >= this.lockDelay)
            {
                Lock();
            }
        }
    }

    private void HardDrop()
    {
        int dropCells = 0;
        while (Move(Vector2Int.down, true))
        {
            dropCells++;
        }

        this.board.score += dropCells * 2;
        Lock();
    }

    private void Lock()
    {
        bool isTSpin = CheckTSpin();

        this.board.Set(this);
        this.enabled = false;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Land);
        }

        this.board.ClearLines(isTSpin);
    }

    public bool Move(Vector2Int translation, bool ignoreSound = false)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;

            bool isGrounded = !board.IsValidPosition(this, position + Vector3Int.down);
            if (isGrounded && lockResets < maxLockResets)
            {
                this.lockTime = 0f;
                this.lockResets++;
            }
            else if (!isGrounded)
            {
                this.lockTime = 0f;
            }

            if (translation.y < 0)
            {
                lastActionWasRotation = false;
            }
            else if (translation.x != 0)
            {
                lastActionWasRotation = false;
            }

            if (!ignoreSound && AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Move);
            }
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        int targetRotation = Wrap(this.rotationIndex + direction, 0, 4);

        Vector3Int[] originalCells = (Vector3Int[])this.cells.Clone();
        ApplySRSState(targetRotation);

        if (TestWallKicks(originalRotation, direction))
        {
            this.rotationIndex = targetRotation;
            this.lastActionWasRotation = true;

            bool isGrounded = !board.IsValidPosition(this, position + Vector3Int.down);
            if (isGrounded && lockResets < maxLockResets)
            {
                this.lockTime = 0f;
                this.lockResets++;
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Rotate);
            }
        }
        else
        {
            this.cells = originalCells;
            this.rotationIndex = originalRotation;
        }
    }

    private void ApplySRSState(int stateIndex)
    {
        Vector2Int[] stateCells = Data.RotationStates[this.data.tetromino][stateIndex];
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)stateCells[i];
        }
    }

    private bool TestWallKicks(int originalRotation, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(originalRotation, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation, true))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int originalRotation, int rotationDirection)
    {
        int wallKickIndex;
        if (rotationDirection > 0)
        {
            wallKickIndex = originalRotation * 2;
        }
        else
        {
            wallKickIndex = originalRotation * 2 - 1;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private bool CheckTSpin()
    {
        if (this.data.tetromino != Tetromino.T || !lastActionWasRotation)
        {
            return false;
        }

        Vector3Int[] corners = new Vector3Int[]
        {
            new Vector3Int(position.x - 1, position.y + 1, 0),
            new Vector3Int(position.x + 1, position.y + 1, 0),
            new Vector3Int(position.x - 1, position.y - 1, 0),
            new Vector3Int(position.x + 1, position.y - 1, 0),
        };

        int occupiedCorners = 0;
        RectInt bounds = board.Bounds;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3Int cornerPos = corners[i];
            if (!bounds.Contains((Vector2Int)cornerPos) || board.tilemap.HasTile(cornerPos))
            {
                occupiedCorners++;
            }
        }

        return occupiedCorners >= 3;
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
