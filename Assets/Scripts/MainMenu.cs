using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator blackFade;

    bool IsAlreadyChangingScene;
    void Update()
    {
        if (Input.anyKeyDown && !IsAlreadyChangingScene)
        {
            if (PlayerPrefs.GetInt("NameSubmitted", 0) == 1)
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Click);
                blackFade.SetTrigger("StartBlackFade");
                Invoke("LoadGame", 1.5f);
                IsAlreadyChangingScene = true;
            }
            else
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Click);
                blackFade.SetTrigger("StartBlackFade");
                Invoke("LoadName", 1.5f);
                IsAlreadyChangingScene = true;
            }
        }
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(2);
    }
    private void LoadName()
    {
        SceneManager.LoadScene(3);
    }
}
