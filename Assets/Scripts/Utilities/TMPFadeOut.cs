using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPFadeOut : MonoBehaviour
{
    [SerializeField] private float delay = 0;
    [SerializeField] private float duration;

    private TextMeshPro _text;
    
    void Start()
    {
        _text = GetComponent<TextMeshPro>();

        StartCoroutine(Fade());
    }
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(delay);
        
        float elapsed = 0;

        Color initColor = _text.color;
        Color finalColor = new Color(initColor.r, initColor.g, initColor.b, 0);

        while (elapsed < duration)
        {
            _text.color = Color.Lerp(initColor, finalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _text.color = finalColor;
    }
}
