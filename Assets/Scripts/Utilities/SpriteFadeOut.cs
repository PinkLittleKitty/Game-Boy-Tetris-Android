using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteFadeOut : MonoBehaviour
{
    [SerializeField] private float delay = 0;
    [SerializeField] private float duration;

    private SpriteRenderer _sprite;
    
    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();

        StartCoroutine(Fade());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(delay);
        
        float elapsed = 0;

        Color initColor = _sprite.color;
        Color finalColor = new Color(initColor.r, initColor.g, initColor.b, 0);

        while (elapsed < duration)
        {
            _sprite.color = Color.Lerp(initColor, finalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _sprite.color = finalColor;
    }
}
