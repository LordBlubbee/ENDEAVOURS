using UnityEngine;

public class UI_OrderMarker : MonoBehaviour
{
    public SpriteRenderer Spr;
    float Scale = 1f;
    private void Update()
    {
        float scale = Scale * 0.6f + CAM.cam.camob.orthographicSize * 0.04f;
        transform.localScale = new Vector3(scale, scale, 1);
    }
}
