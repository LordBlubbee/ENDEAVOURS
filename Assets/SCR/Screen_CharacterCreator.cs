using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_CharacterCreator : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TextMeshProUGUI UsernameTex;
    public Slider UsernameR;
    public Slider UsernameG;
    public Slider UsernameB;
    private void Start()
    {
        UsernameTex.text = GO.g.localUsername;
        UsernameTex.color = GO.g.localColor;
        UsernameR.value = GO.g.localColor.r;
        UsernameG.value = GO.g.localColor.g;
        UsernameB.value = GO.g.localColor.b;
    }
    public void EditUsername()
    {
        GO.g.localUsername = UsernameInput.text;
        UsernameTex.text = GO.g.localUsername;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideR()
    {
        GO.g.localColor = new Color(UsernameR.value, GO.g.localColor.g, GO.g.localColor.b);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideG()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, UsernameG.value, GO.g.localColor.b);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }
    public void EditUsernameSlideB()
    {
        GO.g.localColor = new Color(GO.g.localColor.r, GO.g.localColor.g, UsernameB.value);
        UsernameTex.color = GO.g.localColor;
        GO.g.saveSettings();
    }

    public void PressCreateCharacter()
    {
        LOCALCO.local.CreatePlayerRpc(GO.g.localUsername, GO.g.localColor);
        UI.ui.SelectScreen(UI.ui.MainGameplayUI.gameObject);
    }
}
