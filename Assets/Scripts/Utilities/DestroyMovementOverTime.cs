using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyMovementOverTime : MonoBehaviour
{
    [SerializeField] private float time;

    void Start()
    {
        Destroy(this.GetComponent<SimpleMoveWithSpeed>(), time);
    }

    void Update()
    {
        if (!this.TryGetComponent<SimpleMoveWithSpeed>(out SimpleMoveWithSpeed simpleMoveWithSpeed))
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            SceneManager.LoadScene(1);
        }
    }
}
