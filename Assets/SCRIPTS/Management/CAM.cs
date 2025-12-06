using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CAM : MonoBehaviour
{
    public static CAM cam;
    public Light2D MainLight;
    public Camera camob;
    public ParticleSystem Weather_Drizzle;
    public ParticleSystem Weather_Rain;
    public ParticleSystem Weather_ThunderRain;
    public ParticleSystem Weather_SmallClouds;
    public ParticleSystem Weather_LargeClouds;
    public ParticleSystem Weather_ThunderClouds;
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

    public void SetTimeOfDay(CO.DayTimes time, CO.WeatherTypes weather)
    {
        switch (time)
        {
            case CO.DayTimes.DAY:
                MainLight.color = new Color(1f, 1f, 1f);
                break;
            case CO.DayTimes.DUSK:
                MainLight.color = new Color(0.69f, 0.35f, 0f);
                break;
            case CO.DayTimes.NIGHT:
                MainLight.color = new Color(0.1f, 0.12f, 0.18f);
                break;
        }
        float Factor;
        Weather_Drizzle.gameObject.SetActive(false);
        Weather_Rain.gameObject.SetActive(false);
        Weather_ThunderRain.gameObject.SetActive(false);
        Weather_SmallClouds.gameObject.SetActive(false);
        Weather_LargeClouds.gameObject.SetActive(false);
        Weather_ThunderClouds.gameObject.SetActive(false);
        switch (weather)
        {
            case CO.WeatherTypes.DRIZZLE:
                Weather_Drizzle.gameObject.SetActive(true);
                Weather_SmallClouds.gameObject.SetActive(true);
                break;
            case CO.WeatherTypes.SOME_CLOUDS:
                Weather_SmallClouds.gameObject.SetActive(true);
                break;
            case CO.WeatherTypes.CLOUDY:
                Factor = 0.8f;
                Weather_LargeClouds.gameObject.SetActive(true);
                MainLight.color = new Color(MainLight.color.r * Factor, MainLight.color.g * Factor, MainLight.color.b * Factor);
                break;
            case CO.WeatherTypes.RAIN:
                Weather_Rain.gameObject.SetActive(true);
                Weather_LargeClouds.gameObject.SetActive(true);
                Factor = 0.8f;
                MainLight.color = new Color(MainLight.color.r * Factor, MainLight.color.g * Factor, MainLight.color.b * Factor);
                break;
            case CO.WeatherTypes.THUNDERSTORM:
                Weather_ThunderRain.gameObject.SetActive(true);
                Weather_ThunderClouds.gameObject.SetActive(true);
                Factor = 0.5f;
                MainLight.color = new Color(MainLight.color.r* Factor, MainLight.color.g* Factor, MainLight.color.b* Factor);
                break;
        }
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
            ShakePower -= Time.deltaTime * 4f * Mathf.Max((ShakePower - 1f),1f);
            float Shake = Mathf.Clamp(ShakePower,0,4);
            if (ShakePower > 4f) Shake += (ShakePower - 4f) * 0.2f;
            CameraShake = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-1f, 1f)) * Shake;
            yield return null;
        }
        CameraShake = Vector3.zero;
    }

    public float Dis(Vector3 pos)
    {
        return Vector3.Distance(new Vector3(pos.x, pos.y, 0), new Vector3(transform.position.x, transform.position.y, 0));
    }
}
