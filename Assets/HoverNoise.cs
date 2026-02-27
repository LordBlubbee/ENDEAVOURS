using UnityEngine;
using UnityEngine.EventSystems;

public class HoverNoise : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Hover);
    }
}
