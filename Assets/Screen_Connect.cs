using TMPro;
using UnityEngine;

public class Screen_Connect : MonoBehaviour
{
    public TextMeshProUGUI RefreshTex;

    private void OnEnable()
    {
        if (RefreshUpdate > 0f)
        {
            return;
        }
        LOBBY.lobby.RefreshLobbyList();
        RefreshUpdate = 1.5f;
        RefreshTex.color = Color.gray;
    }
    public void PressHostGame()
    {
        UI.ui.HostUI.AloneGame = false;
        UI.ui.PressSelectScreen(UI.ui.HostUI.gameObject);
    }
    public void PressRefresh()
    {
        if (RefreshUpdate > 0f)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        LOBBY.lobby.RefreshLobbyList();
        RefreshUpdate = 1.5f;
        RefreshTex.color = Color.gray;
    }

    float RefreshUpdate = 0f;
    private void Update()
    {
        if (RefreshUpdate > 0)
        {
            RefreshUpdate -= Time.deltaTime;
            if (RefreshUpdate <= 0) RefreshTex.color = Color.white;
        }
    }
}
