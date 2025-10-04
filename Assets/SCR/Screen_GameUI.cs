using NUnit;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_GameUI : MonoBehaviour
{
    public CanvasGroup ActiveUI;
    public CanvasGroup WeaponUI;
    public Slider HealthSlider;
    public TextMeshProUGUI HealthTex;
    public Image HealthColor;
    public Slider StaminaSlider;
    public TextMeshProUGUI StaminaTex;
    public Image StaminaColor;
    public TextMeshProUGUI InteractTex;
    public GameObject PauseMenu;
    public GameObject CommsMapButton;
    public GameObject InventoryButton;
    public InventorySlot[] InventoryWeaponSlots;
    public InventorySlot InventoryGrappleSlot;
    public InventorySlot InventoryToolsSlot;
    public InventorySlot InventoryHealsSlot;
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
            bool CommsEnabled = CO.co.CommunicationGamePaused.Value;
            CommsMapButton.gameObject.SetActive(CommsEnabled);
            InventoryButton.gameObject.SetActive(false);
            if (CommsEnabled && Input.GetKeyDown(KeyCode.M)) OpenMissionScreen();
        } else
        {
            CommsMapButton.gameObject.SetActive(true);
            InventoryButton.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.I)) UI.ui.SelectScreen(UI.ui.InventoryUI.gameObject);
            if (Input.GetKeyDown(KeyCode.M)) OpenMissionScreen();
        }
        ActiveUI.gameObject.SetActive(true);

        HealthSlider.value = player.GetHealthRelative();
        StaminaSlider.value = player.GetStaminaRelative();
        HealthTex.text = player.GetHealth().ToString("0");
        StaminaTex.text = player.GetStamina().ToString("0");
        HealthColor.color = new Color(1f - player.GetHealthRelative(), player.GetHealthRelative(), 0);
        StaminaColor.color = new Color(1f - player.GetStaminaRelative(), player.GetStaminaRelative(), player.GetStaminaRelative());

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

    public void SetActiveGameUI(CanvasGroup group)
    {
        ActiveUI.gameObject.SetActive(group.gameObject == ActiveUI.gameObject);
        WeaponUI.gameObject.SetActive(group.gameObject == WeaponUI.gameObject);
    }

    private void OnEnable()
    {
        StartCoroutine(RefreshWeaponUINum());
    }

    IEnumerator RefreshWeaponUINum()
    {
        RefreshWeaponUI();
        yield return new WaitForSeconds(0.1f);
        RefreshWeaponUI();
        while (true)
        {
            RefreshWeaponUI();
            yield return new WaitForSeconds(2f);
        }
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
        InventoryGrappleSlot.SetEquipState(ID == -1 ? InventorySlot.EquipStates.SUCCESS : InventorySlot.EquipStates.NONE);
        InventoryToolsSlot.SetEquipState(ID == -2 ? InventorySlot.EquipStates.SUCCESS : InventorySlot.EquipStates.NONE);
        InventoryHealsSlot.SetEquipState(ID == -3 ? InventorySlot.EquipStates.SUCCESS : InventorySlot.EquipStates.NONE);
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
