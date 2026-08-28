using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI[] leaderboardTexts = new TextMeshProUGUI[3];
    [SerializeField] private Animator blackFade;
    [SerializeField] private Leaderboard leaderboard;
    
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
        
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLevel(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLevel(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }
    
    public void ChangeLevel(int direction)
    {
        selectedLevel += direction;
        
        if (selectedLevel < 0)
        {
            selectedLevel = 9;
        }
        else if (selectedLevel > 9)
        {
            selectedLevel = 0;
        }
        
        UpdateLevelDisplay();
    }
    
    public void SetLevel(int level)
    {
        selectedLevel = Mathf.Clamp(level, 0, 9);
        UpdateLevelDisplay();
    }
    
    private void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            levelText.text = selectedLevel.ToString();
        }
    }
    
    public void StartGame()
    {
        if (isChangingScene) return;
        
        isChangingScene = true;
        PlayerPrefs.SetInt("StartingLevel", selectedLevel);
        PlayerPrefs.Save();
        
        StartCoroutine(StartGameCoroutine());
    }
    
    private IEnumerator StartGameCoroutine()
    {
        if (blackFade != null)
        {
            blackFade.Play("FadeToBlack");
            yield return new WaitForSeconds(1.0f);
        }
        
        SceneManager.LoadScene(1);
    }
    
    private void LoadTopScores()
    {
        if (isLoadingScores) return;

        if (leaderboard == null || string.IsNullOrEmpty(leaderboard.publicCode))
        {
            for (int i = 0; i < leaderboardTexts.Length; i++)
            {
                leaderboardTexts[i].text = "Offline";
            }
            return;
        }
        
        for (int i = 0; i < leaderboardTexts.Length; i++)
        {
            leaderboardTexts[i].text = "Loading...";
        }
        
        StartCoroutine(LoadTopScoresCoroutine());
    }
    
    private IEnumerator LoadTopScoresCoroutine()
    {
        isLoadingScores = true;
        
        string baseUrl = (leaderboard != null && !string.IsNullOrEmpty(leaderboard.ApiUrl))
            ? leaderboard.ApiUrl
            : "https://fossboard.justneki.deno.net";

        string url = $"{baseUrl}/lb/{leaderboard.publicCode}/pipe/3";
        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                ParseLeaderboardData(www.downloadHandler.text);
            }
            else
            {
                for (int i = 0; i < leaderboardTexts.Length; i++)
                {
                    leaderboardTexts[i].text = "No scores";
                }
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