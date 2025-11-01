using UnityEngine;

public class BackgroundItem : MonoBehaviour
{

    [Header("Parallax Settings")]
    [Tooltip("How much the background moves compared to the camera. 1 = same speed, 0.5 = half speed, 2 = double speed, etc.")]
    public float parallaxFactor = 0.5f;

    private Vector3 offset; // persistent wrap offset

    void Start()
    {
        offset = transform.position;
    }
    void Update()
    {
        // --- Parallax movement ---
        Vector3 basePos = BackgroundTransform.back.GetPosition() * parallaxFactor;
        Vector3 newPos = basePos + offset;
        transform.position = newPos;

        // --- Wrapping logic ---
        bool wrapped = false;

        if (transform.position.x < -BackgroundTransform.MapSize())
        {
            offset.x += (BackgroundTransform.MapSize() + BackgroundTransform.MapSize()); // shift offset, not actual position
            wrapped = true;
        }
        else if (transform.position.x > BackgroundTransform.MapSize())
        {
            offset.x -= (BackgroundTransform.MapSize() + BackgroundTransform.MapSize());
            wrapped = true;
        }

        if (transform.position.y < -BackgroundTransform.MapSize())
        {
            offset.y += (BackgroundTransform.MapSize() + BackgroundTransform.MapSize());
            wrapped = true;
        }
        else if (transform.position.y > BackgroundTransform.MapSize())
        {
            offset.y -= (BackgroundTransform.MapSize() + BackgroundTransform.MapSize());
            wrapped = true;
        }

        if (wrapped)
        {
            // update position immediately to prevent a 1-frame jump
            transform.position = basePos * parallaxFactor + offset;
        }
    }
}
