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
            
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            blackFade.SetTrigger("StartBlackFade");
            Invoke("LoadGame", 1.5f);
            IsAlreadyChangingScene = true;
        }
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(2);
    }
}
