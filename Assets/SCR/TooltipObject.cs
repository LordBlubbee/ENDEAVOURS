using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class TooltipObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [TextArea(5, 10)]
    public string Tooltip;
    protected bool isHovering = false;
    protected float startTooltip = 0f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        startTooltip = 0f;
    }
    private void Update()
    {
        if (isHovering)
        {
            startTooltip += Time.deltaTime;
            if (startTooltip > 0.3f || TooltipController.tol.isTooltipActive()) TooltipController.tol.setTooltip(Tooltip);
        }
    }
}
