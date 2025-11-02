using NUnit;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_GameUI : MonoBehaviour
{
    public CanvasGroup ActiveUI;
    public CanvasGroup WeaponUI;
  
    public GameObject PauseScreen;

    [Header("CREW STATS")]
    public Slider HealthSlider;
    public TextMeshProUGUI HealthTex;
    public Image HealthColor;
    public Slider StaminaSlider;
    public TextMeshProUGUI StaminaTex;
    public Image StaminaColor;

    [Header("WEAPON STATS")]
    public Slider IntegritySlider;
    public TextMeshProUGUI IntegrityTex;
    public Image IntegrityColor;
    public Slider AmmoSlider;
    public TextMeshProUGUI AmmoTex;
    public Image AmmoColor;
    public GameObject NoAmmoOverlay;

    [Header("BOSS BARS")]
    public Slider OurDrifterIntegritySlider;
    public TextMeshProUGUI OurDrifterIntegrityTex;
    public Image OurDrifterIntegrityColor;
    public Image EnemyIcon;
    public Slider EnemySlider;
    public TextMeshProUGUI EnemyTex;
    public Image EnemyColor;

    [Header("MISC")]
    public TextMeshProUGUI InteractTex;
    public GameObject PauseMenu;
    public GameObject CommsMapButton;
    public TextMeshProUGUI CommsTex;
    public GameObject InventoryButton;
    public InventorySlot[] InventoryWeaponSlots;
    public InventorySlot InventoryGrappleSlot;
    public InventorySlot InventoryToolsSlot;
    public InventorySlot InventoryHealsSlot;

    [Header("BLEED OUT")]
    public Image BleedingOutScreen;
    public TextMeshProUGUI BleedingTex;
    public GameObject BleedingButton;
    public void PressRespawnButton()
    {
        if (LOCALCO.local.GetPlayer().BleedingTime.Value > 0) LOCALCO.local.GetPlayer().RespawnASAPRpc();
    }
    void Update()
    {
        if (!LOCALCO.local)
        {
            return;
        }
        PauseScreen.SetActive(CO.co.CommunicationGamePaused.Value);
        CREW player = LOCALCO.local.GetPlayer();
        if (!player)
        {
            ActiveUI.gameObject.SetActive(false);
            return;
        }
        if (CO.co.AreWeInDanger.Value)
        {
            bool CommsEnabled = CO.co.CommunicationGamePaused.Value && !LOCALCO.local.IsArrivingAnimation();
            /*
               if (UI.ui.RewardUI.RewardActive) UI.ui.OpenTalkScreenFancy(UI.ui.RewardUI.gameObject);
        else if (CO_STORY.co.IsCommsActive()) UI.ui.OpenTalkScreenFancy(UI.ui.TalkUI.gameObject);
        else UI.ui.OpenTalkScreenFancy(UI.ui.MapUI.gameObject);
             
             
             */
            if (CommsEnabled)
            {
                if (UI.ui.RewardUI.RewardActive) CommsTex.text = "[U] REWARDS";
                else if (CO_STORY.co.IsCommsActive()) CommsTex.text = "[U] COMMS";
                else CommsTex.text = "[U] MAP";
            }
            

            CommsMapButton.gameObject.SetActive(CommsEnabled);
            InventoryButton.gameObject.SetActive(false);
            if (CommsEnabled && Input.GetKeyDown(KeyCode.M)) OpenMissionScreen();
        } else
        {
            if (UI.ui.RewardUI.RewardActive) CommsTex.text = "[U] REWARDS";
            else if (CO_STORY.co.IsCommsActive()) CommsTex.text = "[U] COMMS";
            else CommsTex.text = "[U] MAP";

            CommsMapButton.gameObject.SetActive(true);
            InventoryButton.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.I)) UI.ui.OpenTalkScreenFancy(UI.ui.InventoryUI.gameObject);
            if (Input.GetKeyDown(KeyCode.U)) OpenMissionScreen();
        }
        if (ActiveUI.gameObject.activeSelf)
        {
            HealthSlider.value = player.GetHealthRelative();
            HealthTex.text = player.GetHealth().ToString("0");
            HealthColor.color = new Color(1f - player.GetHealthRelative(), player.GetHealthRelative(), 0);
            StaminaSlider.value = player.GetStaminaRelative();
            StaminaTex.text = player.GetStamina().ToString("0");
            StaminaColor.color = new Color(1f - player.GetStaminaRelative(), player.GetStaminaRelative(), player.GetStaminaRelative());
        }
        /*else if (WeaponUI.gameObject.activeSelf)
        {
            ModuleWeapon wep = LOCALCO.local.GetWeapon();
            if (wep)
            {
                IntegritySlider.value = wep.GetHealthRelative();
                IntegrityTex.text = wep.GetHealth().ToString("0");
                IntegrityColor.color = new Color(1f - wep.GetHealthRelative(), wep.GetHealthRelative(), 0);
                AmmoSlider.value = wep.GetAmmoRatio();
                AmmoTex.text = wep.GetAmmo().ToString("");
                AmmoColor.color = new Color(1f - wep.GetAmmoRatio(), wep.GetAmmoRatio(), wep.GetAmmoRatio());
            }
            NoAmmoOverlay.SetActive(wep.LoadedAmmo.Value < 1);
        }*/
        OurDrifterIntegritySlider.gameObject.SetActive(true);
        OurDrifterIntegritySlider.value = CO.co.PlayerMainDrifter.GetHealthRelative();
        OurDrifterIntegrityTex.text = CO.co.PlayerMainDrifter.GetHealth().ToString("0");
        OurDrifterIntegrityColor.color = new Color(1f - CO.co.PlayerMainDrifter.GetHealthRelative(), CO.co.PlayerMainDrifter.GetHealthRelative(), 0);

        if (CO.co.EnemyBarRelative.Value > -1)
        {
            EnemyIcon.gameObject.SetActive(true);
            EnemySlider.value = CO.co.EnemyBarRelative.Value;
            EnemyTex.text = CO.co.EnemyBarString.Value.ToString();
            EnemyColor.color = new Color(1, 0, 0);
        }
        else
        {
            EnemyIcon.gameObject.SetActive(false);
        }

        if (InteractActive)
        {
            InteractTex.color = new Color(InteractTex.color.r, InteractTex.color.g, InteractTex.color.b, Mathf.Clamp01(InteractTex.color.a + Time.deltaTime * 2f));
        }
        else
        {
            InteractTex.color = new Color(InteractTex.color.r, InteractTex.color.g, InteractTex.color.b, Mathf.Clamp01(InteractTex.color.a - Time.deltaTime * 2f));
        }

        if (player.isDead())
        {
            BleedingOutScreen.gameObject.SetActive(true); //!CO.co.PlayerMainDrifter.MedicalModule.IsDisabled()
            BleedingButton.gameObject.SetActive(!player.isDeadForever() && player.Space == CO.co.PlayerMainDrifter.Interior && player.BleedingTime.Value < 40);
            if (player.isDeadForever())
            {
                /*if (CO.co.PlayerMainDrifter.MedicalModule.IsDisabled())
                {
                    BleedingTex.text = "-CANNOT RESPAWN- \nNO MEDICAL BAY";
                } else
                {
                }*/
                if (player.Space != CO.co.PlayerMainDrifter.Interior)
                {
                    BleedingTex.text = "-CANNOT RESPAWN- \nEliminate all threats";
                }
                else
                {
                    BleedingTex.text = $"-RESPAWNING- \n{(20 + player.BleedingTime.Value).ToString("0")}";
                }
            }
            else BleedingTex.text = $"-BLEEDING OUT- \n{player.BleedingTime.Value.ToString("0")}";
            if (UI_CommandInterface.co.IsCommandingTabOpen())
            {
                BleedingOutScreen.color = Color.clear;
            }
            else
            {
                BleedingOutScreen.color = new Color(0.5f, 0, 0, 0.2f);
            }
        } else
        {
            BleedingOutScreen.gameObject.SetActive(false);
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.SetActive(!PauseMenu.activeSelf);
        }
    }

    public void SetActiveGameUI(CanvasGroup group)
    {
        ActiveUI.gameObject.SetActive(group == ActiveUI);
        WeaponUI.gameObject.SetActive(group == WeaponUI);
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
        if (UI.ui.RewardUI.RewardActive) UI.ui.OpenTalkScreenFancy(UI.ui.RewardUI.gameObject);
        else if (CO_STORY.co.IsCommsActive()) UI.ui.OpenTalkScreenFancy(UI.ui.TalkUI.gameObject);
        else UI.ui.OpenTalkScreenFancy(UI.ui.MapUI.gameObject);
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
