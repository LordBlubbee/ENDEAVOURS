using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        } else
        {
            ActiveFade = Mathf.Clamp01(ActiveFade - Time.deltaTime * 4f);
        }
        TooltipBox.alpha = ActiveFade;
        if (ActiveFade == 0f) return;
        // Get mouse position in screen space
        Vector2 mousePos = Input.mousePosition;

        // Tooltip size in pixels
        Vector2 tooltipSize = TooltipTrans.sizeDelta * TooltipTrans.lossyScale;

        // Offset (so it’s not directly under the cursor)
        Vector2 offset = new Vector2(15f, -15f);

        // Default position (to the right of the mouse)
        Vector2 targetPos = mousePos + new Vector2(tooltipSize.x / 2f + offset.x, offset.y);

        // Check if tooltip goes off the right edge → flip to left side
        if (targetPos.x + tooltipSize.x / 2f > Screen.width - 10f)
            targetPos.x = mousePos.x - tooltipSize.x / 2f - offset.x;

        // Clamp vertically to screen bounds
        float halfHeight = tooltipSize.y / 2f;
        targetPos.y = Mathf.Clamp(targetPos.y, halfHeight + 10f, Screen.height - halfHeight - 10f);

        // Convert to world position for your canvas
        Vector3 worldPos = targetPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
                TooltipTrans.parent as RectTransform,
                targetPos,
                Camera.main,
                out worldPos
            );
        TooltipBox.transform.position = worldPos;

        //Vector3 Mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Mouse = new Vector3(Mouse.x, Mouse.y);

        /*Vector3 mouse = Mouse;// + new Vector3((TooltipTrans.rect.size.x / 2f) + 5, -TooltipTrans.rect.size.y / 2f); ;
        Vector3 here = mouse;

        float diff = mouse.x - TooltipTrans.rect.size.x / 2f;
        if (diff < 20) here -= new Vector3(diff - 20, 0);
        diff = mouse.x + TooltipTrans.rect.size.x / 2f;
        if (diff > Screen.width-20) here += new Vector3((Screen.width - 20)-diff, 0);
        //
        diff = mouse.y - TooltipTrans.rect.size.y / 2f;
        if (diff < 20) here -= new Vector3(0,diff - 20);
        diff = mouse.y + TooltipTrans.rect.size.y / 2f;
        if (diff > Screen.height - 20) here += new Vector3(0,(Screen.height - 20) - diff);*/

        //TooltipBox.transform.position = Mouse;
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
