using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NameEntryScreen : MonoBehaviour
{
    public TMP_Text nameText;
    private string playerName = "AAA";
    private int currentIndex = 0;
    private float blinkTimer = 0f;
    private bool showLetter = true;

    private Controls playerInput;

    void Start()
    {
        UpdateNameText();
        playerInput = new Controls();
        playerInput.Enable();
    }

    void Update()
    {
        HandleInput();
        UpdateBlinkTimer();
    }

    void HandleInput()
    {
        if (playerInput.Movement.Right.WasPressedThisFrame())
        {
            currentIndex = (currentIndex + 1) % 3;
            UpdateNameText();
        }
        else if (playerInput.Movement.Left.WasPressedThisFrame())
        {
            currentIndex = (currentIndex - 1 + 3) % 3;
            UpdateNameText();
        }
        else if (playerInput.Movement.Up.WasPressedThisFrame())
        {
            playerName = playerName.Substring(0, currentIndex) + GetNextLetter(playerName[currentIndex]) + playerName.Substring(currentIndex + 1);
            UpdateNameText();
        }
        else if (playerInput.Movement.Down.WasPressedThisFrame())
        {
            playerName = playerName.Substring(0, currentIndex) + GetPreviousLetter(playerName[currentIndex]) + playerName.Substring(currentIndex + 1);
            UpdateNameText();
        }
        else if (playerInput.Movement.Start.WasPressedThisFrame())
        {
            string nameWithoutUnderscore = playerName.Replace("_", "currentIndex");
            PlayerPrefs.SetString("PlayerName", nameWithoutUnderscore);
            PlayerPrefs.SetInt("NameSubmitted", 1);
            SceneManager.LoadScene(2);
        }
    }

    void UpdateNameText()
    {
        string displayedName = playerName;
        if (showLetter)
            displayedName = displayedName;
        else
            displayedName = displayedName.Remove(currentIndex, 1).Insert(currentIndex, "_");
        nameText.text = displayedName;
    }

    void UpdateBlinkTimer()
    {
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= 0.75f)
        {
            blinkTimer = 0f;
            showLetter = !showLetter;
            UpdateNameText();
        }
    }

    char GetNextLetter(char c)
    {
        if (c == ' ')
            return 'A';
        else if (c == 'Z')
            return 'A';
        else
            return (char)(c + 1);
    }

    char GetPreviousLetter(char c)
    {
        if (c == ' ')
            return 'Z';
        else if (c == 'A')
            return ' ';
        else
            return (char)(c - 1);
    }
}
