using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_GameUI : MonoBehaviour
{
    public CanvasGroup ActiveUI;
    public Slider HealthSlider;
    public TextMeshProUGUI HealthTex;
    public Image HealthColor;
    public Slider StaminaSlider;
    public TextMeshProUGUI StaminaTex;
    public Image StaminaColor;
    void Update()
    {
        if (!LOCALCO.local)
        {
            ActiveUI.gameObject.SetActive(false);
            return;
        }
        CREW player = LOCALCO.local.GetPlayer();
        if (!player)
        {
            ActiveUI.gameObject.SetActive(false);
            return;
        }
        ActiveUI.gameObject.SetActive(true);

        HealthSlider.value = player.GetHealthRelative();
        StaminaSlider.value = player.GetStamina()/100f;
        HealthTex.text = player.GetHealth().ToString("0");
        StaminaTex.text = player.GetStamina().ToString("0");
        HealthColor.color = new Color(1f - player.GetHealthRelative(), player.GetHealthRelative(), 0);
        StaminaColor.color = new Color(1f - player.GetStamina(), player.GetStamina(), player.GetStamina());
    }
}
