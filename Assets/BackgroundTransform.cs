using UnityEngine;

public class BackgroundTransform : MonoBehaviour
{
    public static BackgroundTransform back;
    void Start()
    {
        back = this;
    }

    private Vector3 pos = Vector3.zero; 
    public void ResetPosition()
    {
        pos = Vector3.zero;
    }
    public void AddPosition(Vector3 vec)
    {
        pos += vec;
    }
    public Vector3 GetPosition()
    {
        return pos;
    }
    public float MapSize()
    {
        return 600;
    }
}
