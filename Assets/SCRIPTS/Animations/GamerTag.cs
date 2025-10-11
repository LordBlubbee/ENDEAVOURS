using TMPro;
using UnityEngine;

public class GamerTag : MonoBehaviour
{
    public TextMeshPro Name;
    public TextMeshPro Health;
    public SpriteRenderer FarIcon;
    private iDamageable FollowObject;
    private bool UseFarIcon;
    public void SetPlayerAndName(iDamageable trans, string str, Color col)
    {
        //
        FollowObject = trans;
        Name.text = str;
        Name.color = col;
        UseFarIcon = false;
    }
    public void SetObject(iDamageable trans)
    {
        //
        FollowObject = trans;
        UseFarIcon = true;
    }
    public void SetFarIcon(Sprite spr)
    {
        FarIcon.sprite = spr;
    }
    private void Update()
    {
        transform.position = FollowObject.transform.position + new Vector3(0, 2);
        float healthRelative = FollowObject.GetHealthRelative();
        Color col = new Color(1 - healthRelative, healthRelative, 0);
        Health.text = $"{FollowObject.GetHealth().ToString("0")}/{FollowObject.GetMaxHealth().ToString("0")}";
        Health.color = col;

        if (UseFarIcon)
        {
            float far = CAM.cam.camob.orthographicSize;
            float scale = 0.5f + far * 0.01f;
            FarIcon.color = new Color(col.r, col.g, col.b, Mathf.Clamp01(far * 0.04f - 2.2f));
            FarIcon.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
