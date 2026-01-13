using UnityEngine;
using UnityEngine.UI;

public class MuffleButtonController : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float inactiveAlpha = 0f;
    
    private void Start()
    {
        UpdateButtonAppearance();
    }
    
    public void ToggleMuffleSound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ToggleMuffledSound();
            UpdateButtonAppearance();
        }
    }
    
    private void UpdateButtonAppearance()
    {
        if (AudioManager.instance != null)
        {
            bool isMuffled = AudioManager.instance.IsMuffled();
            Color color = buttonImage.color;
            color.a = isMuffled ? activeAlpha : inactiveAlpha;
            buttonImage.color = color;
        }
    }
}