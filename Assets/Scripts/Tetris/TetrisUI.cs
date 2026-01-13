using UnityEngine;
using TMPro;

public class TetrisUI : MonoBehaviour
{
    [Header("Hold Piece UI")]
    public GameObject holdPiecePanel;
    public TextMeshProUGUI holdLabel;
    
    [Header("Next Piece UI")]
    public GameObject nextPiecePanel;
    public TextMeshProUGUI nextLabel;
    
    [Header("Game Info")]
    public Board board;

    private void Update()
    {
        if (holdPiecePanel != null && board != null)
        {
            var canvasGroup = holdPiecePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = board.canHold ? 1.0f : 0.5f;
            }
        }
    }
}