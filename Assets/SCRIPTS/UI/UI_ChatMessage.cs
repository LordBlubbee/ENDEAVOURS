using TMPro;
using UnityEngine;

public class UI_ChatMessage : MonoBehaviour
{
    public TextMeshProUGUI Message;
    public void SetMessage(string message)
    {
        Message.text = message;
        Timeout = 10f + message.Length*0.1f;
    }

    private float Timeout = 0f;
    private float NaturalFade = 1f;
    private void Update()
    {
        Timeout -= Time.deltaTime;
        if (Timeout < 0f)
        {
            NaturalFade -= Time.deltaTime;
        }
        float Fad = NaturalFade;
        if (UI.ui.ChatUI.ChatScreen.activeSelf)
        {
            Fad = 1f;
        }
        Message.color = new Color(Message.color.r, Message.color.g, Message.color.b, Fad);
    }
}
