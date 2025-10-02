using UnityEngine;

public class Hands : MonoBehaviour
{
    public CREW Crew;
    public int HandID;
    public float JointRange = 0.4f;
    public float JointBeyondRangeMod = 0.4f;
    public float JointMove = 15f; // tweak this to control how fast it follows
    private Vector3 OriginalPosition;
    private void Start()
    {
        OriginalPosition = transform.localPosition;
    }
    private void Update()
    {
        Vector3 wantPos;
        Vector3 originalPos = transform.parent.TransformPoint(OriginalPosition);
        if (Crew.EquippedToolObject)
        {
            wantPos = Crew.EquippedToolObject.handPoints[HandID].position;
            wantPos = new Vector3(wantPos.x, wantPos.y);
            float range = (wantPos - originalPos).magnitude;
            float handDistance = Mathf.Min((range - JointRange) * JointBeyondRangeMod + JointRange,range);
            wantPos = originalPos + (wantPos - originalPos).normalized*handDistance;
        } else
        {
            wantPos = originalPos;
        }
        transform.position = Vector3.Lerp(
            transform.position,
            wantPos,
            CO.co.GetWorldSpeedDelta() * JointMove
        );
    }
}
