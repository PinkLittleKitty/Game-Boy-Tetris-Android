using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI[] leaderboardTexts = new TextMeshProUGUI[3];
    [SerializeField] private Animator blackFade;
    [SerializeField] private DreamloLeaderboard dreamloLeaderboard;
    
    private int selectedLevel = 0;
    private bool isLoadingScores = false;
    private bool isChangingScene = false;
    
    void Start()
    {
        selectedLevel = PlayerPrefs.GetInt("StartingLevel", 0);
        UpdateLevelDisplay();
        LoadTopScores();
    }
    
    void Update()
    {
        if (isChangingScene) return;
        
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedLevel = (selectedLevel - 1 + 10) % 10;
            UpdateLevelDisplay();
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedLevel = (selectedLevel + 1) % 10;
            UpdateLevelDisplay();
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBackToMainMenu();
        }
    }
    
    private void UpdateLevelDisplay()
    {
        levelText.text = selectedLevel.ToString();
        PlayerPrefs.SetInt("StartingLevel", selectedLevel);
    }
    
    private void StartGame()
    {
        if (isChangingScene) return;
        
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        blackFade.SetTrigger("StartBlackFade");
        Invoke("LoadGameScene", 1.5f);
        isChangingScene = true;
    }
    
    private void GoBackToMainMenu()
    {
        if (isChangingScene) return;
        
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        blackFade.SetTrigger("StartBlackFade");
        Invoke("LoadMainMenu", 1.5f);
        isChangingScene = true;
    }
    
    private void LoadGameScene()
    {
        SceneManager.LoadScene(2);
    }
    
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    
    private void LoadTopScores()
    {
        if (isLoadingScores) return;
        
        for (int i = 0; i < leaderboardTexts.Length; i++)
        {
            leaderboardTexts[i].text = "Loading...";
        }
        
        StartCoroutine(LoadTopScoresCoroutine());
    }
    
    private IEnumerator LoadTopScoresCoroutine()
    {
        isLoadingScores = true;
        
        string url = "http://dreamlo.com/lb/" + dreamloLeaderboard.publicCode + "/pipe/3";
        
        WWW www = new WWW(url);
        yield return www;
        
        if (string.IsNullOrEmpty(www.error))
        {
            ParseLeaderboardData(www.text);
        }
        else
        {
            for (int i = 0; i < leaderboardTexts.Length; i++)
            {
                leaderboardTexts[i].text = "No scores";
            }
        }
        
        isLoadingScores = false;
    }
    
    private void ParseLeaderboardData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            for (int i = 0; i < leaderboardTexts.Length; i++)
            {
                leaderboardTexts[i].text = "No scores";
            }
            return;
        }
        
        string[] entries = data.Split('\n');
        
        for (int i = 0; i < leaderboardTexts.Length; i++)
        {
            if (i < entries.Length && !string.IsNullOrEmpty(entries[i]))
            {
                string[] parts = entries[i].Split('|');
                if (parts.Length >= 2)
                {
                    string playerName = parts[0];
                    string score = parts[1];
                    leaderboardTexts[i].text = $"{i + 1}. {playerName} - {score}";
                }
                else
                {
                    leaderboardTexts[i].text = $"{i + 1}. No score";
                }
            }
            else
            {
                leaderboardTexts[i].text = $"{i + 1}. No score";
            }
        }
    }
}