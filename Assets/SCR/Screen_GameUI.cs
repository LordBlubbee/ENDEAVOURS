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
    public TextMeshProUGUI InteractTex;
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

        if (InteractActive)
        {
            InteractTex.color = new Color(1, 1, 1, InteractTex.color.a + Time.deltaTime*2f);
        } else
        {
            InteractTex.color = new Color(1, 1, 1, InteractTex.color.a - Time.deltaTime * 2f);
        }
    }

    private bool InteractActive = false;
    public void SetInteractTex(string tex)
    {
        if (tex == "")
        {
            InteractActive = false;
            return;
        }
        InteractActive = true;
        InteractTex.text = tex;
    }
}
