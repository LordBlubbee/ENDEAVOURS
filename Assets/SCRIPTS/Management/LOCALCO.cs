
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class LOCALCO : NetworkBehaviour
{
    public static LOCALCO local;
    public NetworkVariable<int> PlayerID = new NetworkVariable<int>();

    [NonSerialized] public NetworkVariable<int> CurrentDialogVote = new NetworkVariable<int>(-1);
    [NonSerialized] public NetworkVariable<int> CurrentMapVote = new NetworkVariable<int>(-1);

    private CREW Player;
    private DRIFTER Drifter;
    private enum ControlModes
    {
        NONE,
        PLAYER,
        COMMAND,
        DIALOG,
        DRIFTER,
        WEAPON
    }

    private ControlModes CurrentControlMode = ControlModes.NONE;
    public CREW GetPlayer()
    {
        //Works only locally
        return Player;
    }

    public ModuleWeapon GetWeapon()
    {
        return UsingWeapon;
    }
    private void Start()
    {
        StartCoroutine(Register());
        if (IsOwner)
        {
            local = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(Register());
    }

    private IEnumerator Register()
    {
        while (CO.co == null) yield return null;

        CO.co.RegisterLOCALCO(this);
    }
    private void Update()
    {
        //Controls
        if (GetPlayer())
        {
            Vector3 mov = Vector3.zero;
            Vector3 Mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Mouse = new Vector3(Mouse.x, Mouse.y);
            Vector3 CrosshairPoint = Mouse;
     
            if (UI.ui.CurrentlySelectedScreen != UI.ui.MainGameplayUI.gameObject)
            {
                GetPlayer().SetMoveInput(mov);
                GetPlayer().SetMoveInputRpc(Vector3.zero);
                Cursor.visible = true;
                return;
            }
            Cursor.visible = Input.GetKey(KeyCode.LeftControl) || UI.ui.MainGameplayUI.PauseMenu.activeSelf;
            switch (CurrentControlMode)
            {
                case ControlModes.PLAYER:
                    UI.ui.MainGameplayUI.SetActiveGameUI(UI.ui.MainGameplayUI.ActiveUI);
                    bool SpecialCrosshair = GetPlayer().EquippedToolObject;
                    UI.ui.SetCrosshairTexture(SpecialCrosshair ? GetPlayer().EquippedToolObject.CrosshairSprite : null);
                    if (SpecialCrosshair)
                    {
                        float Dist = (Mouse - GetPlayer().transform.position).magnitude;
                        if (Dist < GetPlayer().EquippedToolObject.CrosshairMinRange)
                        {
                            CrosshairPoint = GetPlayer().transform.position + (Mouse-GetPlayer().transform.position).normalized * GetPlayer().EquippedToolObject.CrosshairMinRange;
                        }
                        else if (Dist > GetPlayer().EquippedToolObject.CrosshairMaxRange)
                        {
                            CrosshairPoint = GetPlayer().transform.position + (Mouse - GetPlayer().transform.position).normalized * GetPlayer().EquippedToolObject.CrosshairMaxRange;
                        }
                        if (GetPlayer().EquippedToolObject.RotateCrosshair)
                        {
                            UI.ui.SetCrosshairRotateTowards(CrosshairPoint, GetPlayer().transform.position);
                        } else
                        {
                            UI.ui.SetCrosshairRotateReset();
                        }
                    } else
                    {
                        UI.ui.SetCrosshairRotateReset();
                    }
                    UI.ui.SetCrosshair(CrosshairPoint);
                    if (Input.GetKey(KeyCode.W)) mov += new Vector3(0, 1);
                    if (Input.GetKey(KeyCode.S)) mov += new Vector3(0, -1);
                    if (Input.GetKey(KeyCode.A)) mov += new Vector3(-1, 0);
                    if (Input.GetKey(KeyCode.D)) mov += new Vector3(1, 0);
                    if (!IsServer) GetPlayer().SetMoveInputRpc(mov);
                    if (!IsServer) GetPlayer().SetLookTowardsRpc(Mouse);
                    GetPlayer().SetMoveInput(mov);
                    GetPlayer().SetLookTowards(Mouse);
                    if (Input.GetKeyDown(KeyCode.LeftShift)) GetPlayer().DashRpc();
                    if (Input.GetMouseButtonDown(0)) GetPlayer().UseItem1Rpc();
                    if (Input.GetMouseButtonUp(0)) GetPlayer().StopItem1Rpc();
                    if (Input.GetMouseButtonDown(1)) GetPlayer().UseItem2Rpc();
                    if (Input.GetMouseButtonUp(1)) GetPlayer().StopItem2Rpc();
                   // UI.ui.MainGameplayUI.InventoryGrappleSlot.SetEquipState(InventorySlot.EquipStates.NONE);
                    /*if (Player.Space.isCurrentGridBoardable(Player.transform.position))
                    {
                        if (Input.GetKey(KeyCode.G))
                        {
                            UI.ui.MainGameplayUI.InventoryGrappleSlot.SetEquipState(InventorySlot.EquipStates.SUCCESS);
                            if (Player.GetGrappleCooldown() <= 0 && HasGrappleTarget())
                            {
                                UI.ui.SetCrosshair(GetGrappleTarget(Mouse), UI.CrosshairModes.GRAPPLE_SUCCESS);
                                if (Input.GetKeyUp(KeyCode.G))
                                {
                                    GetPlayer().UseGrappleRpc(Mouse);
                                }
                            }
                            else
                            {
                                UI.ui.SetCrosshair(Mouse, UI.CrosshairModes.GRAPPLE);
                            }
                        }
                    } else
                    {
                        if (Input.GetKey(KeyCode.G))
                        {
                            UI.ui.MainGameplayUI.InventoryGrappleSlot.SetEquipState(InventorySlot.EquipStates.FAIL);
                        }
                    }*/
                    if (Input.GetKeyDown(KeyCode.G)) GetPlayer().EquipGrappleRpc();
                    if (Input.GetKeyDown(KeyCode.E)) GetPlayer().EquipWrenchRpc();
                    if (Input.GetKeyDown(KeyCode.Q)) GetPlayer().EquipMedkitRpc();
                    if (Input.GetKeyDown(KeyCode.Alpha1)) GetPlayer().EquipWeapon1Rpc();
                    if (Input.GetKeyDown(KeyCode.Alpha2)) GetPlayer().EquipWeapon2Rpc();
                    if (Input.GetKeyDown(KeyCode.Alpha3)) GetPlayer().EquipWeapon3Rpc();
                    break;
                case ControlModes.COMMAND:
                    //Managed by the CommandInterface
                    UI.ui.MainGameplayUI.SetActiveGameUI(UI.ui.MainGameplayUI.ActiveUI);
                    UI.ui.SetCrosshairTexture(null);
                    UI.ui.SetCrosshair(CrosshairPoint);
                    UI.ui.SetCrosshairRotateReset();
                    GetPlayer().SetMoveInput(Vector3.zero);
                    if (!IsServer) GetPlayer().SetMoveInputRpc(Vector3.zero);
                    break;
            }
        }
    }
    public bool HasGrappleTarget()
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),0.1f))
        {
            WalkableTile tile = col.GetComponent<WalkableTile>();
            if (tile && tile.Space != Player.Space)
            {
                return true;
            }
        }
        return false;
    }
    public Vector3 GetGrappleTarget(Vector3 def)
    {
        WalkableTile tile = Player.Space.GetNearestBoardingGridTransformToPoint(Player.transform.position);
        if (tile == null) return def;
        return tile.transform.position;
    }

    private iInteractable CurrentInteractionModule = null;
    public int GetPlayerID()
    {
        return PlayerID.Value;
    }
    public void SetPlayerID(int ID)
    {
        PlayerID.Value = ID;
    }
    public void SetPlayerObject(CREW crew)
    {
        Debug.Log("Player set");
        Player = crew;
        if (IsOwner)
        {
            SetCameraToPlayer();
            StartCoroutine(CheckInteraction());
        }
    }

    [Rpc(SendTo.Server)]
    public void CreatePlayerRpc(string name, Color col, int[] attributes, string backTex)
    {
        //SpawnPlayer Spawn Player
        CREW crew = Instantiate(CO_SPAWNER.co.PlayerPrefab, CO.co.PlayerMainDrifter.transform.TransformPoint(CO.co.PlayerMainDrifter.Interior.Bridge), Quaternion.identity);
        crew.NetworkObject.Spawn();
        Debug.Log($"We are player: {GetPlayerID()}");
        crew.PlayerController.Value = GetPlayerID();
        crew.CharacterName.Value = name;
        crew.CharacterNameColor.Value = new Vector3(col.r, col.g, col.b);
        crew.UpdateAttributes(
            attributes[0],
            attributes[1],
            attributes[2],
            attributes[3],
            attributes[4],
            attributes[5],
            attributes[6],
            attributes[7]
            );
        ScriptableBackground back = Resources.Load<ScriptableBackground>(backTex);
        crew.SetCharacterBackground(back); //Must be called BEFORE INIT
        crew.Init();
        crew.RegisterPlayerOnLOCALCORpc();
        Player.EquipWeapon(0, back.Background_StartingWeapon);
        Player.EquipWeaponPrefab(0);
        CO.co.PlayerMainDrifter.Interior.AddCrew(crew);
    }
    public void SetCameraToPlayer()
    {
        if (CurrentControlMode == ControlModes.PLAYER) return;
        CurrentControlMode = ControlModes.PLAYER;
        CAM.cam.SetCameraMode(Player.transform, 13f+ Player.GetATT_COMMUNOPATHY(), 8f, 16f+Player.GetATT_COMMUNOPATHY());
    }

    public void SetCameraToCommand()
    {
        if (CurrentControlMode == ControlModes.COMMAND) return;
        CurrentControlMode = ControlModes.COMMAND;
        CAM.cam.SetCameraMode(CO.co.PlayerMainDrifter.transform, 150f, 30f, 150f);
    }

    Transform DraggingObject;
    ModuleWeapon UsingWeapon;
    IEnumerator CheckInteraction()
    {
        float Timer = 0f;
        //Collider2D[] ColliderList = null;
        iInteractable mod = null;
        while (true)
        {
            UI.ui.MainGameplayUI.SetInteractTex("", Color.white);
            if (DraggingObject != null)
            {
                UI.ui.MainGameplayUI.SetInteractTex("[F] DROP", Color.white);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    DraggingObject = null;
                    GetPlayer().StopDragging();
                }
            }
            else if (CurrentControlMode == ControlModes.PLAYER && Player.Space != null)
            {
               
                Timer -= CO.co.GetWorldSpeedDelta();
                if (Timer < 0f)
                {
                    Timer = 0.25f; 
                    float maxRange = 10f;
                    mod = null;
                    foreach (Collider2D col in Physics2D.OverlapCircleAll(Player.transform.position + Player.getLookVector()*1.5f, 2f))
                    {
                        iInteractable crate = col.GetComponent<iInteractable>();
                        float dis = (col.transform.position - Player.transform.position).magnitude;
                        if (crate != null && dis < maxRange)
                        {
                            maxRange = dis;
                            mod = crate;
                        }
                    }
                    /*mod = Player.Space.NearestModule(Player.transform.position);

                    if (mod == null || (mod.transform.position - Player.transform.position).magnitude > 5f || Mathf.Abs(Player.AngleBetweenPoints(mod.transform.position)) > 45f)
                    {
                        mod = null; 
                        float maxRange = 10f;
                        foreach (Collider2D col in Physics2D.OverlapCircleAll(Player.transform.position + Player.getLookVector(), 1f))
                        {
                            iInteractable crate = col.GetComponent<iInteractable>();
                            float dis = (col.transform.position - Player.transform.position).magnitude;
                            if (crate != null && dis < maxRange)
                            {
                                maxRange = dis;
                                mod = crate;
                            }
                        }
                    }*/
                }
                if (mod != null)
                {
                    if (mod.IsDisabled())
                    {
                        if (mod is Module) UI.ui.MainGameplayUI.SetInteractTex("[DISABLED]", Color.red);
                    }
                    else
                    {
                        switch (mod.GetInteractableType())
                        {
                            /*case Module.ModuleTypes.NAVIGATION:
                                UI.ui.MainGameplayUI.SetInteractTex("[F] NAVIGATE", Color.green);
                                if (Input.GetKeyDown(KeyCode.F))
                                {
                                    //Interact
                                    Drifter = Player.Space.Drifter;
                                    CurrentControlMode = ControlModes.DRIFTER;
                                    CAM.cam.SetCameraMode(Drifter.transform, 100f, 50f, 150);
                                    CurrentInteractionModule = mod;
                                }

                                break;
                            case Module.ModuleTypes.WEAPON:
                                UI.ui.MainGameplayUI.SetInteractTex("[F] USE WEAPON", Color.green);
                                if (Input.GetKeyDown(KeyCode.F))
                                {

                                    UsingWeapon = (ModuleWeapon)mod;
                                    CurrentControlMode = ControlModes.WEAPON;
                                    CAM.cam.SetCameraMode(mod.transform, 100f, 50f, 150);
                                    CurrentInteractionModule = mod;
                                }
                                break;*/
                            case Module.ModuleTypes.WEAPON:
                                ModuleWeapon weapon = mod as ModuleWeapon;
                                string str;

                                if (weapon.ReloadingCurrently.Value)
                                {
                                    str = $"{weapon.ModuleTag}\nRELOADING...";
                                } else
                                {
                                    if (weapon.GetAmmoRatio() < 0.1f) str = $"{weapon.ModuleTag}\nAMMO ({weapon.GetAmmo() / weapon.MaxAmmo})";
                                    else str = $"{weapon.ModuleTag}\nAMMO ({weapon.GetAmmo()}/{weapon.MaxAmmo})";
                                    if (weapon.GetAmmoRatio() < 1f)
                                    {
                                        if (CO.co.Resource_Ammo.Value < 10) str += "\n<color=red> [NO AMMO]";
                                        else str += "\n<color=green> [F] RELOAD <color=red> [10 AMMO]";
                                        if (Input.GetKeyDown(KeyCode.F))
                                        {
                                            weapon.ReloadAmmoRpc();
                                        }
                                    }
                                }
                               
                                UI.ui.MainGameplayUI.SetInteractTex(str, Color.yellow);
                                break;
                            case Module.ModuleTypes.INVENTORY:
                                UI.ui.MainGameplayUI.SetInteractTex("[F] INVENTORY", Color.green);
                                if (Input.GetKeyDown(KeyCode.F))
                                    UI.ui.SelectScreen(UI.ui.InventoryUI.gameObject);
                                break;
                            case Module.ModuleTypes.MAPCOMMS:
                                if (!CO.co.AreWeInDanger.Value)
                                {
                                    if (CO_STORY.co.IsCommsActive())
                                    {
                                        UI.ui.MainGameplayUI.SetInteractTex("[F] OPEN COMMS", Color.cyan);
                                        if (Input.GetKeyDown(KeyCode.F))
                                        {
                                            UI.ui.SelectScreen(UI.ui.TalkUI.gameObject);
                                        }
                                    }
                                    else
                                    {
                                        UI.ui.MainGameplayUI.SetInteractTex("[F] OPEN MAP", Color.cyan);
                                        if (Input.GetKeyDown(KeyCode.F))
                                        {
                                            UI.ui.SelectScreen(UI.ui.MapUI.gameObject);
                                        }
                                    }
                                }
                                else
                                {
                                    UI.ui.MainGameplayUI.SetInteractTex("[DANGER]", Color.red);
                                }
                                break;
                            /*case Module.ModuleTypes.GENERATOR:
                                UI.ui.MainGameplayUI.SetInteractTex("[F] MANAGE", Color.green);
                                if (Input.GetKeyDown(KeyCode.F))
                                {

                                }
                                break;*/
                            case Module.ModuleTypes.DRAGGABLE:
                                UI.ui.MainGameplayUI.SetInteractTex("[F] CARRY", Color.green);
                                if (Input.GetKeyDown(KeyCode.F))
                                {
                                    DraggingObject = mod.transform;
                                    GetPlayer().StartDragging(mod.transform);
                                }
                                break;
                        }
                    }
                }
            } else if (Input.GetKeyDown(KeyCode.F) || LeftModule())
            {
                if (CurrentInteractionModule != null)
                {
                    switch (CurrentInteractionModule.GetInteractableType())
                    {
                        case Module.ModuleTypes.WEAPON:
                            UsingWeapon.Stop();
                            break;
                    }
                }
                CurrentInteractionModule = null;
                SetCameraToPlayer();
            }
            yield return null;
        }
    }
    private bool LeftModule()
    {
        if (CurrentInteractionModule != null)
        {
            if ((CurrentInteractionModule.transform.position - Player.transform.position).magnitude > 6f || Player.isDead()) return true;
        }
        return false;
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void ShipTransportFadeAwayRpc(string str)
    {
        UI.ui.FadeToBlack(1f,1f);
        UI.ui.SetCinematicTex(str, Color.green, 4f, 1f);
        CAM.cam.SetCameraCinematic(200f);
        UI.ui.SelectScreen(null);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShipTransportFadeInRpc()
    {
        UI.ui.FadeFromBlack(2f);
        UI.ui.SelectScreen(UI.ui.TalkUI.gameObject);
        LOCALCO.local.SetCameraToPlayer();
    }
}
