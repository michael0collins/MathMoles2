using UnityEngine;
using UnityEngine.EventSystems;

public delegate void ButtonPressed();

public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public ButtonPressed ButtonPressed;
    public bool isPressedDown = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressedDown = true;
        if (ButtonPressed != null)
            ButtonPressed.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressedDown = false;
    }
}
