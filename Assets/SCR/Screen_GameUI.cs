using NUnit;
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
    public GameObject PauseMenu;
    public GameObject SafeMenu;
    public InventorySlot[] InventoryWeaponSlots;
    public InventorySlot InventoryGrappleSlot;
    public InventorySlot InventoryToolsSlot;
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
        if (CO.co.AreWeInDanger.Value)
        {
            SafeMenu.gameObject.SetActive(false);
        } else
        {
            SafeMenu.gameObject.SetActive(true);
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
            InteractTex.color = new Color(InteractTex.color.r, InteractTex.color.g, InteractTex.color.b, Mathf.Clamp01(InteractTex.color.a + Time.deltaTime * 2f));
        } else
        {
            InteractTex.color = new Color(InteractTex.color.r, InteractTex.color.g, InteractTex.color.b, Mathf.Clamp01(InteractTex.color.a - Time.deltaTime * 2f));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.SetActive(!PauseMenu.activeSelf);
        }
    }

    private void OnEnable()
    {
        RefreshWeaponUI();
    }
    public void OpenMissionScreen()
    {
        if (CO_STORY.co.IsCommsActive()) UI.ui.SelectScreen(UI.ui.TalkUI.gameObject);
        else UI.ui.SelectScreen(UI.ui.MapUI.gameObject);
    }
    public void EquipWeaponUI(int ID)
    {
        for (int i = 0; i < InventoryWeaponSlots.Length; i++)
        {
            InventoryWeaponSlots[i].SetEquipState(ID == i ? InventorySlot.EquipStates.SUCCESS : InventorySlot.EquipStates.NONE);
        }
    }
    public void RefreshWeaponUI()
    {
        if (!LOCALCO.local.GetPlayer())
        {
            return;
        }
        for (int i = 0; i < InventoryWeaponSlots.Length; i++)
        {
            InventoryWeaponSlots[i].SetInventoryItem(LOCALCO.local.GetPlayer().EquippedWeapons[i]);
        }
    }

    private bool InteractActive = false;
    public void SetInteractTex(string tex, Color col)
    {
        if (tex == "")
        {
            InteractActive = false;
            return;
        }
        InteractActive = true;
        InteractTex.text = tex;
        InteractTex.color = col;
    }
}
