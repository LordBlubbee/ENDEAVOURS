using TMPro;
using UnityEngine;

public class GamerTag : MonoBehaviour
{
    public TextMeshPro Text;
    private Transform FollowObject;
    public void SetPlayerAndName(Transform trans, string str, Color col)
    {
        FollowObject = trans;
        Text.text = str;
        Text.color = col;
    }

    private void Update()
    {
        transform.position = FollowObject.transform.position + new Vector3(0, 2);
    }
}
