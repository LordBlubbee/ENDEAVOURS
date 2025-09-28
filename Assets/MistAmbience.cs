using System.Collections;
using UnityEngine;

public class MistAmbience : MonoBehaviour
{
    private void Start()
    {
        
    }

    IEnumerator RunAmbience()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            break;
        }
    }
}
