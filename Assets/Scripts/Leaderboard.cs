using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Leaderboard : MonoBehaviour
{
    [Header("FOSSBoard Config")]
    public string apiUrl = "https://fossboard.justneki.deno.net";
    public string privateCode = "";
    public string publicCode = "";

    private int lastPersonalBest = 0;

    public string ApiUrl => string.IsNullOrEmpty(apiUrl) ? "https://fossboard.justneki.deno.net" : apiUrl.TrimEnd('/');
    public string PublicCode => publicCode;

    private void Start()
    {
        lastPersonalBest = PlayerPrefs.GetInt("personalBest", 0);
    }

    public void UploadScore(int score)
    {
        if (score > lastPersonalBest)
        {
            lastPersonalBest = score;
            PlayerPrefs.SetInt("personalBest", lastPersonalBest);

            StartCoroutine(UploadScoreCoroutine(score));
        }
    }

    private IEnumerator UploadScoreCoroutine(int score)
    {
        if (string.IsNullOrEmpty(privateCode))
        {
            Debug.LogWarning("Leaderboard: privateCode is not set; score not uploaded.");
            yield break;
        }

        string playerName = PlayerPrefs.GetString("PlayerName", "AAA");

        string url = $"{ApiUrl}/lb/{privateCode}/add/{UnityWebRequest.EscapeURL(playerName)}/{score}/0/";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score uploaded successfully: " + score);
            }
            else
            {
                Debug.LogError("Error uploading score: " + www.error);
            }
        }
    }
}
