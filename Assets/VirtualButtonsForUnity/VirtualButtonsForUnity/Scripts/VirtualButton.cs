using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool pressed = false;
    public Sprite sprite;
    private Image image;

    void Start()
    {
        image = this.gameObject.GetComponent<Image>();
    }

    void FixedUpdate()
    {
        if (pressed)
        {
            image.sprite = sprite;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 100f);

        }
        else
        {
            this.gameObject.GetComponent<Image>().sprite = null;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        }
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        pressed = false;
    }
}
