using TMPro;
using UnityEngine;

public class UI_OrderMarker : MonoBehaviour
{
    public SpriteRenderer Spr;
    public Rotator Rotator;
    public TextMeshPro Texto;
    float Scale = 1f;

    public void SetNumber(string num)
    {
        Texto.text = num;
    }
    public void SelectOrderMarker()
    {
        Spr.color = new Color(0,1,0,0.7f);
        Rotator.RotationSpeed = 30;
    }
    public void DeselectOrderMarker()
    {
        Spr.color = new Color(1, 1, 0, 0.7f);
        Rotator.RotationSpeed = 90;
    }
    private void Update()
    {
        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale, scale, 1);
    }
}
