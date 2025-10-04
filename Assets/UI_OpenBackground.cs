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
    private void OnEnable()
    {
        Icon.sprite = Background.MainIcon;
        ShortDesc.text = Background.ShortDesc;
        Title.text = Background.BackgroundName;
        Title.color = Background.BackgroundColor;
    }
    public void WhenPressed()
    {
        UI.ui.CharacterCreationUI.OpenExactBackground(this);
    }
}
