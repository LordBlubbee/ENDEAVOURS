using UnityEngine;
using UnityEngine.UI;

public class Screen_MainMenu : MonoBehaviour
{
    public GameObject MainMenuContents;
    public Image BlackBlocker;
    void Update()
    {
        BlackBlocker.color = new Color(0, 0, 0, Mathf.Clamp01(BlackBlocker.color.a - Time.deltaTime));
    }
    private void Start()
    {
        EnableMainMenu(false);
    }
    public void EnableMainMenu(bool bol)
    {
        MainMenuContents.SetActive(bol);
    }
    private void OnEnable()
    {
        UI.ui.CinematicUI.PlayIntroCinematic();
    }
    public void PressPlayAlone()
    {
        UI.ui.HostUI.AloneGame = true;
        UI.ui.PressSelectScreen(UI.ui.HostUI.gameObject);
    }
}
