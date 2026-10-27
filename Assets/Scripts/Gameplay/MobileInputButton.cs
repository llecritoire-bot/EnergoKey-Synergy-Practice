using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 direction;
    private PlayerController target;

    public void Configure(PlayerController player, Vector2 value)
    {
        target = player;
        direction = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        target?.SetMobileDirection(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        target?.ClearMobileDirection(direction);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target?.ClearMobileDirection(direction);
    }
}

