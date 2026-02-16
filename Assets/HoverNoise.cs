using UnityEngine;
using UnityEngine.EventSystems;

public class HoverNoise : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip SFX;
    public void OnPointerEnter(PointerEventData eventData)
    {
        AUDCO.aud.PlaySFX(SFX);
    }
}
