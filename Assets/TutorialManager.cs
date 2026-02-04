using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI TutorialTitle;
    public TextMeshProUGUI TutorialDescription;
    public GameObject ContinueButton;
    public GameObject SkipButton;
    private TutorialProgressLevels Progress;
    private enum TutorialProgressLevels
    {
        INTRO1,
        INTRO2,
        WASD,
        DASH,
        LMB1_ATTACK,
        LMB2_ATTACK,
        SELECT_WEAPONS,
        Q_MEDKIT,
        E_WRENCH,
        G_GRAPPLE,
        X_ACCESSWEAPONS_OPEN,
        X_ACCESSWEAPONS_TARGET,
        X_ACCESSWEAPONS_CANCEL,
        C_ACCESSCREW_OPEN,
        C_ACCESSCREW_TARGET,
        C_ACCESSCREW_CANCEL,
        Z_ACCESSMODULES,
        COMMAND_INFO,
        I_OPENINVENTORY,
        EQUIP_ITEM,
        UPGRADE_ENGINES,
        ADDITIONAL_INVENTORY_INFO,
        ADDITIONAL_RESOURCES,
        ADDITIONAL_INVENTORY_TECH,
        CLOSE_INVENTORY,
        U_OPENCOMMS,
        DIPLOMACY,
        OUTRO1,
        ENDING
    }

    public void OpenTutorial()
    {
        if (!GO.g.enableTutorial) return;
        gameObject.SetActive(true);
        Progress = TutorialProgressLevels.INTRO1;
        SetTutorial();
    }
    public void ProgressTutorial()
    {
        Progress++;
        SetTutorial();
    }
    IEnumerator ContinueAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        ContinueButton.SetActive(true);
    }
    public void SetTutorial()
    {
        TutorialEngagementProgress = 0;
        UpdateDescription();
        ContinueButton.SetActive(false);
        switch (Progress)
        {
            case TutorialProgressLevels.INTRO1:
                StartCoroutine(ContinueAfterDelay());
                SkipButton.SetActive(true);
               break;
            case TutorialProgressLevels.INTRO2:
                StartCoroutine(ContinueAfterDelay());
                SkipButton.SetActive(true);
                break;
            case TutorialProgressLevels.WASD:
                SkipButton.SetActive(false);
                break;
            case TutorialProgressLevels.DASH:
                break;
            case TutorialProgressLevels.LMB1_ATTACK:
                break;
            case TutorialProgressLevels.LMB2_ATTACK:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.SELECT_WEAPONS:
                break;
            case TutorialProgressLevels.Q_MEDKIT:
                break;
            case TutorialProgressLevels.E_WRENCH:
                break;
            case TutorialProgressLevels.G_GRAPPLE:
                break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_OPEN:
                 break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_TARGET:
                break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_CANCEL:
                break;
            case TutorialProgressLevels.C_ACCESSCREW_OPEN:
                break;
            case TutorialProgressLevels.C_ACCESSCREW_TARGET:
                break;
            case TutorialProgressLevels.C_ACCESSCREW_CANCEL:
                break;
            case TutorialProgressLevels.Z_ACCESSMODULES:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.COMMAND_INFO:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.I_OPENINVENTORY:
                break;
            case TutorialProgressLevels.EQUIP_ITEM:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.UPGRADE_ENGINES:
                break;
            case TutorialProgressLevels.ADDITIONAL_INVENTORY_INFO:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.ADDITIONAL_RESOURCES:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.ADDITIONAL_INVENTORY_TECH:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.CLOSE_INVENTORY:
                break;
            case TutorialProgressLevels.U_OPENCOMMS:
                break;
            case TutorialProgressLevels.DIPLOMACY:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.OUTRO1:
                StartCoroutine(ContinueAfterDelay());
                break;
            case TutorialProgressLevels.ENDING:
                GO.g.enableTutorial = false;
                GO.g.saveSettings();
                gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateDescription()
    {
        switch (Progress)
        {
            case TutorialProgressLevels.INTRO1:
                TutorialTitle.text = "TUTORIAL";
                TutorialDescription.text = "Welcome to the POST-ENDEAVOUR MISSION BRIEFING. As a commanding officer in the last remaining Endeavour Drifter, knowledge in both combat and command are essential.";
                break;
            case TutorialProgressLevels.INTRO2:
                TutorialTitle.text = "INSTRUCTIONS";
                TutorialDescription.text = "Are you ready to begin a small course? Press [Y] to cancel the training, and [SPACE] to begin instructions.";
                break;
            case TutorialProgressLevels.WASD:
                TutorialTitle.text = "MOVEMENT";
                TutorialDescription.text = $"To move around, use WASD. You move faster in the direction you are looking at with your mouse.\n\n<color=yellow>Move around: ({TutorialEngagementProgress}/6)</color>";
                break;
            case TutorialProgressLevels.DASH:
                TutorialTitle.text = "DASHING";
                TutorialDescription.text = $"Press SHIFT to dash, granting you a quick burst of movement. Dashing uses your STAMINA, which is the blue bar. Dashing allows you to quickly reload and deal additional melee damage.\n\n<color=yellow>Dash: ({TutorialEngagementProgress}/4)</color>";
                break;
            case TutorialProgressLevels.LMB1_ATTACK:
                TutorialTitle.text = "ATTACK";
                TutorialDescription.text = $"Use your LEFT MOUSE BUTTON to use your current items. For most weapons, this means that you will perform an ATTACK SEQUENCE.\n\n<color=yellow>Attack: ({TutorialEngagementProgress}/4)</color>";
                break;
            case TutorialProgressLevels.LMB2_ATTACK:
                TutorialTitle.text = "SECONDARY ACTION";
                TutorialDescription.text = "Use your RIGHT MOUSE BUTTON to use your item's secondary effects. Melee weapons can often use this to BLOCK attacks. BLOCKING attacks this way can grant you a PARRY damage bonus.";
                break;
            case TutorialProgressLevels.SELECT_WEAPONS:
                TutorialTitle.text = "WEAPON SELECTION";
                TutorialDescription.text = $"Press ONE, TWO, or THREE to equip your different items.\n\n<color=yellow>Equip items: ({TutorialEngagementProgress}/3)</color>";
                break;
            case TutorialProgressLevels.Q_MEDKIT:
                TutorialTitle.text = "MEDKIT USAGE";
                TutorialDescription.text = $"Press Q to equip your medkit. LEFT MOUSE BUTTON heals allies in front of you, while RIGHT MOUSE BUTTON heals yourself. Healing allies is far more efficient. MEDICAL skill improves healing.\n\n<color=yellow>Use Medkit: ({TutorialEngagementProgress}/1)</color>";
                break;
            case TutorialProgressLevels.E_WRENCH:
                TutorialTitle.text = "REPAIR USAGE";
                TutorialDescription.text = $"Press E to equip your tools. LEFT MOUSE BUTTON allows you to repair damaged modules, which keeps your Drifter afloat. ENGINEERING skill improves repairing.\n\n<color=yellow>Use Wrench: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.G_GRAPPLE:
                TutorialTitle.text = "GRAPPLE USAGE";
                TutorialDescription.text = $"Press G to equip your Grappling Hook. LEFT MOUSE BUTTON allows you to fire a projectile that can attach to different Drifters or platforms, allowing you to move to another area. GRAPPLING has a cooldown.\n\n<color=yellow>Use Grapple: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_OPEN:
                TutorialTitle.text = "DRIFTER CONTROLS";
                TutorialDescription.text = "Press X to open the Command Interface. In particular, the DRIFTER WEAPONS MANAGER. Your Drifter's weapons are essential in destroying critical modules on other Drifters and providing firing support to your team.";
                break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_TARGET:
                TutorialTitle.text = "TARGETING ENEMIES";
                TutorialDescription.text = $"Here, you can use the NUMBER KEYS to select a weapon. When you have a weapon selected, use LEFT MOUSE BUTTON to manually select a target.\n\n<color=yellow>Aim Weapon 1: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.X_ACCESSWEAPONS_CANCEL:
                TutorialTitle.text = "TARGETING ENEMIES";
                TutorialDescription.text = $"Press LEFT MOUSE BUTTON and then RIGHT MOUSE BUTTON to cancel manual targeting. When set on AUTO, your crew will automatically reload the weapon and it will automatically shoot nearby enemies.\n\n<color=yellow>Cancel Aim Weapon 1: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.C_ACCESSCREW_OPEN:
                TutorialTitle.text = "CREW CONTROLS";
                TutorialDescription.text = "Press C to open the Command Interface's CREW tab. Here, you can view your Drifter's crew.";
                break;
            case TutorialProgressLevels.C_ACCESSCREW_TARGET:
                TutorialTitle.text = "ORDERING CREW";
                TutorialDescription.text = $"Here, you can use the NUMBER KEYS to select a crew member. Similarly to targeting weapons, you can use LEFT MOUSE BUTTON here to manually order your crew to move somewhere.\n\n<color=yellow>Order Crew 1: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.C_ACCESSCREW_CANCEL:
                TutorialTitle.text = "ORDERING CREW";
                TutorialDescription.text = $"Your crew will automatically perform actions at a targeted area. Similarly to targeting weapons, press LEFT MOUSE BUTTON and RIGHT MOUSE BUTTON to cancel manual orders.\n\n<color=yellow>Cancel Order Crew 1: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.Z_ACCESSMODULES:
                TutorialTitle.text = "MODULE CONTROLS";
                TutorialDescription.text = "Press Z to open the Command Interface's MODULES tab. Here, you can view and access your Drifter's other modules.";
                break;
            case TutorialProgressLevels.COMMAND_INFO:
                TutorialTitle.text = "COMMAND INTERFACE";
                TutorialDescription.text = "Press Z, X, or C again to exit the Command Interface. Using this Interface is essential to victory. It is recommended to designate a crew member to manage the Drifter's weapons and crew.";
                break;
            case TutorialProgressLevels.I_OPENINVENTORY:
                TutorialTitle.text = "INVENTORY";
                TutorialDescription.text = "Press I to open your Armory. This shortcut works only outside of combat, and allows you to manage your character and your Drifter.";
                break;
            case TutorialProgressLevels.EQUIP_ITEM:
                TutorialTitle.text = "MANAGE ITEMS";
                TutorialDescription.text = "In the EQUIPMENT tab, you can manage your items. You can equip up to three Weapons, up to three Artifacts, and one piece of Armor. You can also salvage unwanted equipment by dragging it to the bottom-right.";
                break;
            case TutorialProgressLevels.UPGRADE_ENGINES:
                TutorialTitle.text = "MANAGE DRIFTER";
                TutorialDescription.text = $"Upgrading the Drifter is a top priority. In the MODULES and WEAPONS tabs, you can upgrade your modules, or install any new modules you purchased. Upgrading Engines, for example, increases your Drifter's chance to dodge incoming fire.\n\n<color=yellow>Spend any Materials: ({TutorialEngagementProgress}/1)";
                break;
            case TutorialProgressLevels.ADDITIONAL_INVENTORY_INFO:
                TutorialTitle.text = "MANAGE OTHERS";
                TutorialDescription.text = "You can also view your crewmates to promote them, increasing their stats, level-up your character once you acquire enough Experience Points, and you can repair your Drifter and craft new items when reaching resting points.";
                break;
            case TutorialProgressLevels.ADDITIONAL_RESOURCES:
                TutorialTitle.text = "RESOURCES";
                TutorialDescription.text = "All of this is done with the four main Resources, seen in the top-left. <color=yellow>Materials</color> are needed to craft and upgrade systems. <color=green>Supplies</color> are needed to upgrade crew and to respawn when you are slain in dangerous dungeons. <color=red>Ammo</color> is needed for your weapons, and <color=0088FF>Tech</color> is a rare resource for Drifter upgrades.";
                break;
            case TutorialProgressLevels.ADDITIONAL_INVENTORY_TECH:
                TutorialTitle.text = "ARMORY STRATEGY";
                TutorialDescription.text = "Upgrading the correct systems and using your Resources well is the key to victory.";
                break;
            case TutorialProgressLevels.CLOSE_INVENTORY:
                TutorialTitle.text = "COMMS";
                TutorialDescription.text = "Now you know how to control yourself and your Drifter. Exit the Armory, that we might go to the Comms.";
                break;
            case TutorialProgressLevels.U_OPENCOMMS:
                TutorialTitle.text = "COMMS";
                TutorialDescription.text = "Press U to open the Mission interface. You progress the game in this screen. Here, you can view rewards, make story choices, and select your next destination.";
                break;
            case TutorialProgressLevels.DIPLOMACY:
                TutorialTitle.text = "DIPLOMACY";
                TutorialDescription.text = "The choices you make choose how you take risk, and where you fight. Attacking innocents reduces your reputation and can have severe consequences. Choose wisely.";
                break;
            case TutorialProgressLevels.OUTRO1:
                TutorialTitle.text = "STRATEGY";
                TutorialDescription.text = "The Expedition has failed. The world has changed, and many dangers lurk ahead. Experience will be your strongest weapon. \n\n<color=green>Go out there and begin your mission. Good luck.</color>";
                break;
            default:
                TutorialTitle.text = "";
                TutorialDescription.text = "";
                break;
        }
    }
    public void PressContinue()
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        ProgressTutorial();
    }

    public void PressSkip()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        SetTutorial();
    }

    int TutorialEngagementProgress = 0;
    bool TutorialEngagementFlip = false;

    private void ContinueEngagement(int required)
    {
        TutorialEngagementProgress++;
        if (TutorialEngagementProgress >= required)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
            ContinueButton.gameObject.SetActive(true);
        }
        UpdateDescription();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (SkipButton.activeSelf) PressSkip();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ContinueButton.activeSelf) PressContinue();
        }
        if (!ContinueButton.activeSelf)
        {
            CREW Player = LOCALCO.local.GetPlayer();
            TOOL tol;
            switch (Progress)
            {
                case TutorialProgressLevels.WASD:
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                    {
                        ContinueEngagement(6);
                    }
                    break;
                case TutorialProgressLevels.DASH:
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        ContinueEngagement(4);
                    }
                    break;
                case TutorialProgressLevels.LMB1_ATTACK:
                    tol = Player.EquippedToolObject;
                    if (tol)
                    {
                        if (!Player.CanStrike())
                        {
                            if (!TutorialEngagementFlip)
                            {
                                TutorialEngagementFlip = true;
                                ContinueEngagement(4);
                            }
                        } else
                        {
                            if (TutorialEngagementFlip) TutorialEngagementFlip = false;
                        }
                    }
                    break;
                case TutorialProgressLevels.SELECT_WEAPONS:
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        ContinueEngagement(3);
                    }
                    break;
                case TutorialProgressLevels.Q_MEDKIT:
                    tol = Player.EquippedToolObject;
                    if (tol)
                    {
                        Debug.Log("ACTION USE: "+tol.ActionUse1);
                        if (tol.ActionUse1 == TOOL.ToolActionType.HEAL_OTHERS)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                ContinueEngagement(1);
                            }
                        }
                    }
                    break;
                case TutorialProgressLevels.E_WRENCH:
                    tol = Player.EquippedToolObject;
                    if (tol)
                    {
                        if (tol.ActionUse1 == TOOL.ToolActionType.REPAIR)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                ContinueEngagement(1);
                            }
                        }
                    }
                    break;
                case TutorialProgressLevels.G_GRAPPLE:
                    if (Player.SlotGrappleCooldown.Value > 0)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.X_ACCESSWEAPONS_OPEN:
                    if (UI_CommandInterface.co.GetSelectedTab() == 1)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.X_ACCESSWEAPONS_TARGET:
                    if (UI_CommandInterface.co.Weapons.Count > 0)
                    {
                        UI_Module wep = UI_CommandInterface.co.Weapons[0];
                        if (wep)
                        {
                            if (wep.OrderMarker.gameObject.activeSelf) ContinueEngagement(1);
                        }
                    } else
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.X_ACCESSWEAPONS_CANCEL:
                    if (UI_CommandInterface.co.Weapons.Count > 0)
                    {
                        UI_Module wep = UI_CommandInterface.co.Weapons[0];
                        if (wep)
                        {
                            if (!wep.OrderMarker.gameObject.activeSelf) ContinueEngagement(1);
                        }
                    }
                    else
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.C_ACCESSCREW_OPEN:
                    if (UI_CommandInterface.co.GetSelectedTab() == 2)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.C_ACCESSCREW_TARGET:
                    if (UI_CommandInterface.co.Crews.Count > 0)
                    {
                        UI_Module wep = UI_CommandInterface.co.Crews[0];
                        if (wep)
                        {
                            if (!wep.OrderMarker.gameObject.activeSelf) ContinueEngagement(1);
                        }
                    }
                    else
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.C_ACCESSCREW_CANCEL:
                    if (UI_CommandInterface.co.Crews.Count > 0)
                    {
                        UI_Module wep = UI_CommandInterface.co.Crews[0];
                        if (wep)
                        {
                            if (!wep.OrderMarker.gameObject.activeSelf) ContinueEngagement(1);
                        }
                    }
                    else
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.Z_ACCESSMODULES:
                    if (UI_CommandInterface.co.GetSelectedTab() == 0)
                    {
                        ContinueEngagement(1);
                    }
                    break;
               
                case TutorialProgressLevels.I_OPENINVENTORY:
                    if (UI.ui.InventoryUI.gameObject.activeSelf)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.UPGRADE_ENGINES:
                    if (CO.co.Resource_Materials.Value < 50)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.CLOSE_INVENTORY:
                    if (UI.ui.MainGameplayUI.gameObject.activeSelf)
                    {
                        ContinueEngagement(1);
                    }
                    break;
                case TutorialProgressLevels.U_OPENCOMMS:
                    if (UI.ui.TalkUI.gameObject.activeSelf || UI.ui.RewardUI.gameObject.activeSelf || UI.ui.MapUI.gameObject.activeSelf)
                    {
                        ContinueEngagement(1);
                    }
                    break;
            }
        }
    }
}
