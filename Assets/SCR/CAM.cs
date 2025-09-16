using UnityEngine;

public class CAM : MonoBehaviour
{
    public static CAM cam;
    public Camera camera;
    private void Awake()
    {
        cam = this;
    }

    Vector3 CameraPosMain;
    private void Update()
    {
        if (LOCALCO.local == null) return;
        if (LOCALCO.local.GetPlayer() == null) return;

        CameraPosMain = LOCALCO.local.GetPlayer().transform.position;
        Vector3 Offset = (camera.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f));
        transform.position = CameraPosMain + new Vector3(Offset.x*30f, Offset.y*20f);
        transform.position = new Vector3(transform.position.x, transform.position.y, -100);
    }
}
