using System.Collections;
using UnityEngine;

public class CAM : MonoBehaviour
{
    public static CAM cam;
    public Camera camob;
    private Transform FollowObject;
    private Vector3 FollowVector;
    private Vector3 CameraShake = Vector3.zero;
    private float TargetOrtho = 15f;
    private float minOrtho = 5f;
    private float maxOrtho = 20f;
    private bool canScroll = false;
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
        canScroll = true;
    }
    public void SetCameraMode(Vector3 followTarget, float mod, float min, float max)
    {
        FollowVector = followTarget;
        TargetOrtho = mod;
        minOrtho = min;
        maxOrtho = max;
        canScroll = true;
    }
    public void SetCameraCinematic(float mod)
    {
        TargetOrtho = mod;
        minOrtho = mod;
        maxOrtho = mod;
    }
    Vector3 CameraPosMain;
    private void Update()
    {
        if (LOCALCO.local == null) return;
        if (FollowObject != null)
        {
            FollowVector = FollowObject.transform.position;
        }

        CameraPosMain = Vector3.Lerp(CameraPosMain, FollowVector, 4f * Time.deltaTime);
        Vector3 Offset = camob.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f);
        transform.position = CameraPosMain + new Vector3(Offset.x * 3f * camob.orthographicSize, Offset.y * 2f * camob.orthographicSize);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1000) + CameraShake;

        if (canScroll)
        {
            float Scroll = Input.mouseScrollDelta.y * -50f * (10f + TargetOrtho * 0.25f) * Time.deltaTime;
            TargetOrtho = Mathf.Clamp(TargetOrtho + Scroll, minOrtho, maxOrtho);
            if (TargetOrtho < 30) playerZoom = TargetOrtho;
            else farZoom = TargetOrtho;
        }
        if (TargetOrtho != camob.orthographicSize)
        {
            if (TargetOrtho > camob.orthographicSize) camob.orthographicSize = Mathf.Clamp(camob.orthographicSize+((TargetOrtho - camob.orthographicSize) * 4f + 10f) * Time.deltaTime,4,TargetOrtho);
            else camob.orthographicSize = Mathf.Clamp(camob.orthographicSize + ((TargetOrtho - camob.orthographicSize) * 4f - 10f) * Time.deltaTime, TargetOrtho, 9999);
        }
    }

    float playerZoom = 15f;
    float farZoom = 100f;

    public float GetPlayerZoom()
    {
        return playerZoom;
    }
    public float GetFarZoom()
    {
        return farZoom;
    }

    bool isShaking = false;
    float ShakePower = 0f;
    public void ShakeCamera(float power)
    {
        ShakePower = power;
        if (!isShaking) StartCoroutine(ShakingCamera());
    }
    IEnumerator ShakingCamera()
    {
        while (ShakePower > 0f)
        {
            ShakePower -= Time.deltaTime * 4f * Mathf.Max((ShakePower - 2f),1);
            CameraShake = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-1f, 1f)) * ShakePower;
            yield return null;
        }
        CameraShake = Vector3.zero;
    }

    public float Dis(Vector3 pos)
    {
        return Vector3.Distance(new Vector3(pos.x, pos.y, 0), new Vector3(transform.position.x, transform.position.y, 0));
    }
}
