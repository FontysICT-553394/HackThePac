using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject hoverIndicator;
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        hoverIndicator.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        hoverIndicator.SetActive(false);
    }
}
