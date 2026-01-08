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
            if (Tooltip == "") return;
            if (!IsPointerStillOverThis())
            {
                isHovering = false;
                return;
            }
            startTooltip += Time.deltaTime;
            if (startTooltip > 0.3f || TooltipController.tol.isTooltipActive()) TooltipController.tol.setTooltip(Tooltip);
        }
    }
    private bool IsPointerStillOverThis()
    {
        if (!EventSystem.current) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            if (r.gameObject == gameObject)
                return true;
        }

        return false;
    }
}
