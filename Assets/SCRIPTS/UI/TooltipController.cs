using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public static TooltipController tol;
    public RectTransform TooltipTrans;
    public CanvasGroup TooltipBox;
    public TextMeshProUGUI TooltipText;

    protected float ActiveFade = 0f;
    protected float TooltipDuration = 0f;
    private void Start()
    {
        tol = this;
    }
    private void Update()
    {
        if (TooltipDuration > 0f)
        {
            TooltipDuration -= Time.deltaTime;
            ActiveFade = Mathf.Clamp01(ActiveFade + Time.deltaTime * 4f);
        }
        else ActiveFade = Mathf.Clamp01(ActiveFade - Time.deltaTime * 4f);

        TooltipBox.alpha = ActiveFade;
        if (ActiveFade == 0f) return;

        RectTransform rt = TooltipTrans;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Vector2 tooltipSize = rt.rect.size;//Vector2.Scale(rt.rect.size, rt.lossyScale);

        Vector2 mousePos = Input.mousePosition;
        Vector2 offset = new Vector2(15f, -15f);

        // Start to the right of the mouse
        Vector2 targetPos = mousePos + offset;

        // Compute edges assuming targetPos is tooltip center
        float halfW = tooltipSize.x * 0.5f;
        float halfH = tooltipSize.y * 0.5f;
        float margin = 10f;

        // Right-edge flip
        //Debug.Log($"{targetPos.x} + {halfW} tries to be above 1920");
        if (targetPos.x + halfW + halfW > Screen.width - margin)
            targetPos.x = mousePos.x - offset.x - tooltipSize.x;

        // Clamp to screen horizontally
        targetPos.x = Mathf.Clamp(targetPos.x, halfW + margin, Screen.width - halfW - margin);
        // Clamp vertically
        targetPos.y = Mathf.Clamp(targetPos.y, halfH + margin, Screen.height - halfH - margin);

        // Convert to world
        Camera cam = Camera.main;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rt.parent as RectTransform,
            targetPos,
            cam,
            out Vector3 worldPos);

        TooltipBox.transform.position = worldPos;
    }



    public bool isTooltipActive()
    {
        return TooltipDuration > 0f;
    }

    public void setTooltip(string tex)
    {
        TooltipText.text = tex;
        TooltipDuration = 0.2f;
    }
}
