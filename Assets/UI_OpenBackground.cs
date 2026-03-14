using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_OpenBackground : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI ShortDesc;
    public Image Border;
    public ScriptableBackground Background;
    public TooltipObject Tool;
    bool IsUnlocked = false;
    private void OnEnable()
    {
        Icon.sprite = Background.Sprite_Player;
        Title.text = Background.BackgroundName;
        Title.color = Background.BackgroundColor;

        if (GO.g.UnlockedBackgrounds.Contains(Background.name))
        {
            IsUnlocked = true;
            ShortDesc.text = Background.ShortDesc;
            Tool.Tooltip = $"{Background.BackgroundName}\n\n{Background.LongDesc}";
        } else
        {
            IsUnlocked = false;
            Tool.Tooltip = Background.HowToUnlockDesc;
            ShortDesc.text = "LOCKED";
            ShortDesc.color = Color.gray;
            Icon.color = Color.gray;
            Title.color = Color.gray;
        }
    }
    public void WhenPressed()
    {
        if (!IsUnlocked)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        UI.ui.CharacterCreationUI.OpenExactBackground(this);
    }
}
