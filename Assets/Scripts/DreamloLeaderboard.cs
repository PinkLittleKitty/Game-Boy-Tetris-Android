using UnityEngine;
using System.Collections;

public class DreamloLeaderboard : MonoBehaviour
{
    public string privateCode = "";
    public string publicCode = "";
    private const string webURL = "http://dreamlo.com/lb/";

    private int lastPersonalBest = 0;

    private void Start()
    {
        PlayerPrefs.SetInt("PersonalBest", 0);
        Debug.Log("PB: " + lastPersonalBest);
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
        string url = webURL + privateCode + "/add/" + WWW.EscapeURL(SystemInfo.deviceUniqueIdentifier) + "/" + score;

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
