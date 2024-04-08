using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using CandyCoded.HapticFeedback;


public enum VirtualDPadDirection { Both, Horizontal, Vertical }

public class VirtualDPad : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform centerArea = null;
    [SerializeField] private RectTransform handle = null;
    [SerializeField] private VirtualDPadDirection direction = VirtualDPadDirection.Both;
    [InputControl(layout = "Vector2")]
    [SerializeField] private string dPadControlPath;
    [SerializeField] private float movementRange = 10f;
    [SerializeField] private float moveThreshold = 0f;
    [SerializeField] private float uiMovementRange = 10f;
    [SerializeField] private bool forceIntValue = true;
    [SerializeField] private Image[] directionImages;

    private Vector3 startPos;

    protected override string controlPathInternal
    {
        get => dPadControlPath;
        set => dPadControlPath = value;
    }

    private void Awake()
    {
        if (centerArea == null)
            centerArea = GetComponent<RectTransform>();

        Vector2 center = new Vector2(0.5f, 0.5f);
        centerArea.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
    }

    private void Start()
    {
        startPos = handle.anchoredPosition;

        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        OnDrag(eventData);
        HapticFeedback.LightFeedback();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(handle.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
        Vector2 delta = position;

        if (direction == VirtualDPadDirection.Horizontal) delta.y = 0;
        else if (direction == VirtualDPadDirection.Vertical) delta.x = 0;

        Vector2 buttonDelta = Vector2.ClampMagnitude(delta, uiMovementRange);
        handle.anchoredPosition = startPos + (Vector3)buttonDelta;

        Vector2 newPos = SanitizePosition(delta);
        SendValueToControl(newPos);

        ToggleDirectionImages(newPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        SendValueToControl(Vector2.zero);

        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    private Vector2 SanitizePosition(Vector2 pos)
    {
        pos = Vector2.ClampMagnitude(pos, movementRange);

        float minMovementRange = this.moveThreshold > movementRange ? movementRange : this.moveThreshold;
        if (pos.x < minMovementRange && pos.x > (minMovementRange * -1)) pos.x = 0;
        if (pos.y < minMovementRange && pos.y > (minMovementRange * -1)) pos.y = 0;

        pos = new Vector2(pos.x / movementRange, pos.y / movementRange);

        if (forceIntValue)
        {
            if (pos.x < 0) pos.x = -1;
            else if (pos.x > 0) pos.x = 1;

            if (pos.y < 0) pos.y = -1;
            else if (pos.y > 0) pos.y = 1;
        }

        return pos;
    }

    private void ToggleDirectionImages(Vector2 direction)
    {
        // Turn off all images
        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }

        // Turn on the appropriate image based on the direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal direction
            if (direction.x > 0)
            {
                // Right image
                directionImages[0].gameObject.SetActive(true);
            }
            else if (direction.x < 0)
            {
                // Left image
                directionImages[1].gameObject.SetActive(true);
            }
        }
        else
        {
            // Vertical direction
            if (direction.y > 0)
            {
                // Up image
                directionImages[2].gameObject.SetActive(true);
            }
            else if (direction.y < 0)
            {
                // Down image
                directionImages[3].gameObject.SetActive(true);
            }
        }
    }
}
