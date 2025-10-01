using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LOCALCO : NetworkBehaviour
{
    public static LOCALCO local;
    private NetworkVariable<int> PlayerID = new NetworkVariable<int>();

    [NonSerialized] public NetworkVariable<int> CurrentDialogVote = new NetworkVariable<int>(-1);
    [NonSerialized] public NetworkVariable<int> CurrentMapVote = new NetworkVariable<int>(-1);

    private CREW Player;
    private DRIFTER Drifter;
    private enum ControlModes
    {
        NONE,
        PLAYER,
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
    private void Start()
    {
        StartCoroutine(Register());
        if (IsOwner)
        {
            local = this;
        }
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
            UI.ui.SetCrosshair(Mouse, UI.CrosshairModes.NONE);
            if (UI.ui.CurrentlySelectedScreen != UI.ui.MainGameplayUI.gameObject)
            {
                GetPlayer().SetMoveInputRpc(Vector3.zero);
                return;
            }
            switch (CurrentControlMode)
            {
                case ControlModes.PLAYER:
                    
                    if (Input.GetKey(KeyCode.W)) mov += new Vector3(0, 1);
                    if (Input.GetKey(KeyCode.S)) mov += new Vector3(0, -1);
                    if (Input.GetKey(KeyCode.A)) mov += new Vector3(-1, 0);
                    if (Input.GetKey(KeyCode.D)) mov += new Vector3(1, 0);
                    GetPlayer().SetMoveInputRpc(mov);
                    GetPlayer().SetLookTowardsRpc(Mouse);
                    if (Input.GetKeyDown(KeyCode.LeftShift)) GetPlayer().DashRpc();
                    if (Input.GetMouseButtonDown(0)) GetPlayer().UseItem1Rpc();
                    if (Input.GetMouseButtonDown(1)) GetPlayer().UseItem2Rpc();
                    if (Input.GetMouseButtonUp(0)) GetPlayer().StopItem1Rpc();
                    if (Input.GetMouseButtonUp(1)) GetPlayer().StopItem2Rpc();
                    UI.ui.MainGameplayUI.InventoryGrappleSlot.SetEquipState(InventorySlot.EquipStates.NONE);
                    if (Player.space.isCurrentGridBoardable(Player.transform.position))
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
                    }
                   
                    if (Input.GetKeyDown(KeyCode.Alpha1)) GetPlayer().EquipWeapon1Rpc();
                    if (Input.GetKeyDown(KeyCode.Alpha2)) GetPlayer().EquipWeapon2Rpc();
                    if (Input.GetKeyDown(KeyCode.Alpha3)) GetPlayer().EquipWeapon3Rpc();
                    break;
                case ControlModes.DRIFTER:
                    GetPlayer().SetMoveInputRpc(Vector3.zero);
                    if (Input.GetKey(KeyCode.W)) mov += new Vector3(0, 1);
                    if (Input.GetKey(KeyCode.S)) mov += new Vector3(0, -1);
                    if (Input.GetKey(KeyCode.A)) mov += new Vector3(-1, 0);
                    if (Input.GetKey(KeyCode.D)) mov += new Vector3(1, 0);
                    Drifter.SetMoveInputRpc(mov,0.8f+GetPlayer().GetATT_PILOTING()*0.1f);
                    Drifter.SetLookTowardsRpc(Mouse);
                    break;
                case ControlModes.WEAPON:
                    UI.ui.SetCrosshair(Mouse, UI.CrosshairModes.WEAPONS);
                    GetPlayer().SetMoveInputRpc(Vector3.zero);
                    UsingWeapon.SetLookTowardsRpc(Mouse);
                    if (Input.GetMouseButtonDown(0)) UsingWeapon.FireRpc(Mouse);
                    break;
            }
        }
    }
    public bool HasGrappleTarget()
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),0.1f))
        {
            WalkableTile tile = col.GetComponent<WalkableTile>();
            if (tile && tile.Space != Player.space)
            {
                return true;
            }
        }
        return false;
    }
    public Vector3 GetGrappleTarget(Vector3 def)
    {
        WalkableTile tile = Player.space.GetNearestBoardingGridTransformToPoint(Player.transform.position);
        if (tile == null) return def;
        return tile.transform.position;
    }

    private Module CurrentInteractionModule = null;
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
        CREW crew = Instantiate(CO_SPAWNER.co.PlayerPrefab, CO.co.PlayerMainDrifter.Interior.StartingModuleLocations[0], Quaternion.identity);
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
        crew.SetCharacterBackground(back);
        crew.Init();
        crew.RegisterPlayerOnLOCALCORpc();
        Player.EquipWeapon(0, back.Background_StartingWeapon);
        Player.EquipWeaponPrefab(0);
        CO.co.PlayerMainDrifter.Interior.AddCrew(crew);
    }
    public void SetCameraToPlayer()
    {
        CurrentControlMode = ControlModes.PLAYER;
        CAM.cam.SetCameraMode(Player.transform, 13f+ Player.GetATT_COMMUNOPATHY(), 8f, 16f+Player.GetATT_COMMUNOPATHY());
    }

    ModuleWeapon UsingWeapon;
    IEnumerator CheckInteraction()
    {
        while (true)
        {
            UI.ui.MainGameplayUI.SetInteractTex("", Color.white);
            if (CurrentControlMode == ControlModes.PLAYER && Player.space != null)
            {
                Module mod = Player.space.NearestModule(Player.transform.position);
                if ((mod.transform.position-Player.transform.position).magnitude < 5f && Mathf.Abs(Player.AngleBetweenPoints(mod.transform.position)) < 45f)
                {
                    switch (mod.ModuleType)
                    {
                        case Module.ModuleTypes.NAVIGATION:
                            UI.ui.MainGameplayUI.SetInteractTex("[F] NAVIGATE", Color.green);
                            if (Input.GetKeyDown(KeyCode.F))
                            {
                                //Interact
                                Drifter = Player.space.Drifter;
                                CurrentControlMode = ControlModes.DRIFTER;
                                CAM.cam.SetCameraMode(Drifter.transform, 230f + Player.GetATT_COMMUNOPATHY() * 10f, 50f, 250f + Player.GetATT_COMMUNOPATHY() * 10f);
                                CurrentInteractionModule = mod;
                            }
                           
                            break;
                        case Module.ModuleTypes.WEAPON:
                            UI.ui.MainGameplayUI.SetInteractTex("[F] USE WEAPON", Color.green);
                            if (Input.GetKeyDown(KeyCode.F))
                            {
                                UsingWeapon = (ModuleWeapon)mod;
                                CurrentControlMode = ControlModes.WEAPON;
                                CAM.cam.SetCameraMode(mod.transform, 230f + Player.GetATT_COMMUNOPATHY() * 10f, 50f, 250f + Player.GetATT_COMMUNOPATHY() * 10f);
                                CurrentInteractionModule = mod;
                            }
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
                            } else
                            {
                                UI.ui.MainGameplayUI.SetInteractTex("[DANGER]", Color.red);
                            }
                            break;
                        case Module.ModuleTypes.GENERATOR:
                            UI.ui.MainGameplayUI.SetInteractTex("[F] MANAGE", Color.green);
                            if (Input.GetKeyDown(KeyCode.F))
                            {

                            }
                            break;
                        case Module.ModuleTypes.ARMOR_MODULE:
                            UI.ui.MainGameplayUI.SetInteractTex("[F] MANAGE", Color.green);
                            if (Input.GetKeyDown(KeyCode.F))
                            {

                            }
                            break;
                    }
                   
                }
            } else if (Input.GetKeyDown(KeyCode.F) || LeftModule())
            {
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
