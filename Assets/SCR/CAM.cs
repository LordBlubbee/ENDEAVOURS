using UnityEngine;

public class CAM : MonoBehaviour
{
    public static CAM cam;
    public Camera camob;
    private Transform FollowObject;
    private float TargetOrtho;
    private float minOrtho = 5f;
    private float maxOrtho = 20f;
    private void Awake()
    {
        cam = this;
    }
    public void SetCameraMode(Transform followTarget, float mod, float min, float max)
    {
        FollowObject = followTarget;
        TargetOrtho = mod;
        minOrtho = min;
        maxOrtho = max;
    }
    Vector3 CameraPosMain;
    private void Update()
    {
        if (LOCALCO.local == null) return;
        if (FollowObject == null) return;

        CameraPosMain = LOCALCO.local.GetPlayer().transform.position;
        Vector3 Offset = (camob.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f));
        transform.position = CameraPosMain + new Vector3(Offset.x*3f * camob.orthographicSize, Offset.y*2f * camob.orthographicSize);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1000);

        if (TargetOrtho != camob.orthographicSize)
        {
            if (TargetOrtho > camob.orthographicSize) camob.orthographicSize = Mathf.Clamp(camob.orthographicSize+((TargetOrtho - camob.orthographicSize) * 4f + 10f) * Time.deltaTime,0,TargetOrtho);
            else camob.orthographicSize = Mathf.Clamp(camob.orthographicSize + ((TargetOrtho - camob.orthographicSize) * 4f - 10f) * Time.deltaTime, TargetOrtho, 9999);
        }
        float Scroll = Input.mouseScrollDelta.y * -50f * (10f + TargetOrtho * 0.25f) * Time.deltaTime;
        TargetOrtho = Mathf.Clamp(TargetOrtho + Scroll, minOrtho, maxOrtho);
    }
}
