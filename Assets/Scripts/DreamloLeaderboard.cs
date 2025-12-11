using UnityEngine;
using System.Collections;

public class DreamloLeaderboard : MonoBehaviour
{
    public string privateCode = "";
    public string publicCode = "";
    private const string webURL = "http://dreamlo.com/lb/";

    private int lastPersonalBest = 0;

    public string PublicCode => publicCode;

    private void Start()
    {
        lastPersonalBest = PlayerPrefs.GetInt("PersonalBest", 0);
    }

    public void UploadScore(int score)
    {
        if (score > lastPersonalBest)
        {
            lastPersonalBest = score;
            PlayerPrefs.SetInt("PersonalBest", lastPersonalBest);

            StartCoroutine(UploadScoreCoroutine(score));
        }
    }

    private IEnumerator UploadScoreCoroutine(int score)
    {
        string url = webURL + privateCode + "/add/" + PlayerPrefs.GetString("PlayerName") + "/" + score;

        WWW www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Score uploaded successfully: " + score);
        }
        else
        {
            Debug.LogError("Error uploading score: " + www.error);
        }
    }
}
