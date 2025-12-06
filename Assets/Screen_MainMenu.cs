using UnityEngine;
using UnityEngine.UI;

public class Screen_MainMenu : MonoBehaviour
{
    public Image BlackBlocker;
    void Update()
    {
        BlackBlocker.color = new Color(0, 0, 0, Mathf.Clamp01(BlackBlocker.color.a - Time.deltaTime));
    }
}
