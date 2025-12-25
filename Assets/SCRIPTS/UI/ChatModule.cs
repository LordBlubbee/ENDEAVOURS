
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatModule : MonoBehaviour
{
    public UI_ChatMessage ChatMessagePrefab;
    public GameObject ChatScreen;
    public TMP_InputField ChatInputField;
    public RectTransform ChatGrid;
    private List<UI_ChatMessage> ChatMessages = new();

    private void Start()
    {
        ChatInputField.onDeselect.AddListener(OnChatInputDeselected);
    }
    public void CreateChatMessage(string message)
    {
        // UI_ChatMessage chat = Instantiate(ChatMessagePrefab, ChatGrid.transform.position,Quaternion.identity);
        //chat.transform.SetParent(ChatGrid);
        // chat.transform.localScale = Vector3.one;
        UI_ChatMessage chat = Instantiate(ChatMessagePrefab);
        chat.transform.localPosition = new Vector3(chat.transform.localPosition.x, chat.transform.localPosition.y, 0);
        chat.transform.SetParent(ChatGrid, false);
        CAM.cam.camob.orthographicSize += 0.001f;
        chat.SetMessage(message);
        ChatMessages.Add(chat);
        if (ChatMessages.Count > 16)
        {
            Destroy(ChatMessages[0].gameObject);
            ChatMessages.RemoveAt(0);
        }
        StartCoroutine(UpdateCanvas());
    }

    IEnumerator UpdateCanvas()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(ChatGrid);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(ChatGrid);
    }
    private void Update()
    {
        if (ChatScreen.activeSelf)
        {
            ChatInputField.Select();
            ChatInputField.ActivateInputField();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (ChatScreen.activeSelf)
            {
                if (ChatInputField.text.Length > 0)
                {
                    string hex2 = ColorUtility.ToHtmlStringRGB(LOCALCO.local.GetPlayer().GetCharacterColor());
                    string Sender = $"<color=#{hex2}>{LOCALCO.local.GetPlayer().CharacterName.Value}</color>";
                    string MessageForSelf = $"{Sender}: {ChatInputField.text}";
                    CreateChatMessage(MessageForSelf);
                    string MessageForOthers = MessageForSelf;
                    LOCALCO.local.SetChatMessageToEveryoneElseRpc(MessageForOthers);
                }
                ChatInputField.text = "";
                ChatScreen.SetActive(false);
            } else
            {
                ChatScreen.SetActive(true);
          
            }
        }
    }
    private void OnChatInputDeselected(string _)
    {
        ChatInputField.text = "";
        ChatScreen.SetActive(false);
    }
}
