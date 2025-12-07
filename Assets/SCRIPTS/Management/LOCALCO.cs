
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private IEnumerator Register()
    {
        while (CO.co == null) yield return null;

        CO.co.RegisterLOCALCO(this);
    }
    private void Update()
    {
        //Controls
        if (!IsOwner) return;
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
                if (Input.GetMouseButtonUp(0)) GetPlayer().StopItem1Rpc();
                if (Input.GetMouseButtonUp(1)) GetPlayer().StopItem2Rpc();
                Cursor.visible = true;
                return;
            }
            Cursor.visible = Input.GetKey(KeyCode.LeftControl) || UI.ui.MainGameplayUI.PauseMenu.activeSelf;
            UI.ui.SetCrosshairColor(Color.white);
            switch (CurrentControlMode)
            {
                case ControlModes.PLAYER:
                    UI.ui.MainGameplayUI.SetActiveGameUI(UI.ui.MainGameplayUI.ActiveUI);
                    bool SpecialCrosshair = GetPlayer().EquippedToolObject;
                    if (GetPlayer().isDead())
                    {
                        UI.ui.SetCrosshairTexture(null);
                    }
                    else UI.ui.SetCrosshairTexture(SpecialCrosshair ? GetPlayer().EquippedToolObject.CrosshairSprite : null);
                    if (SpecialCrosshair)
                    {
                        float Dist = (Mouse - GetPlayer().transform.position).magnitude;
                        if (Dist < GetPlayer().EquippedToolObject.CrosshairMinRange)
                        {
                            CrosshairPoint = GetPlayer().transform.position + (Mouse - GetPlayer().transform.position).normalized * GetPlayer().EquippedToolObject.CrosshairMinRange;
                        }
                        else if (Dist > GetPlayer().EquippedToolObject.CrosshairMaxRange)
                        {
                            CrosshairPoint = GetPlayer().transform.position + (Mouse - GetPlayer().transform.position).normalized * GetPlayer().EquippedToolObject.CrosshairMaxRange;
                        }
                        if (GetPlayer().EquippedToolObject.RotateCrosshair)
                        {
                            UI.ui.SetCrosshairRotateTowards(CrosshairPoint, GetPlayer().transform.position);
                        }
                        else
                        {
                            UI.ui.SetCrosshairRotateReset();
                        }
                        if (GetPlayer().EquippedToolObject.CrosshairSprite == CO_SPAWNER.co.GrappleCursor)
                        {
                            if (HasGrappleTarget()) UI.ui.SetCrosshairColor(Color.green);
                            else UI.ui.SetCrosshairColor(Color.red);
                        }
                    }
                    else
                    {
                        UI.ui.SetCrosshairRotateReset();
                    }
                    UI.ui.SetCrosshair(CrosshairPoint);

                    if (!UI.ui.ChatUI.ChatScreen.activeSelf)
                    {
                        if (Input.GetKey(KeyCode.W)) mov += new Vector3(0, 1);
                        if (Input.GetKey(KeyCode.S)) mov += new Vector3(0, -1);
                        if (Input.GetKey(KeyCode.A)) mov += new Vector3(-1, 0);
                        if (Input.GetKey(KeyCode.D)) mov += new Vector3(1, 0);
                    }


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
                    if (!UI.ui.ChatUI.ChatScreen.activeSelf)
                    {
                        if (Input.GetKeyDown(KeyCode.G)) GetPlayer().EquipGrappleRpc();
                        if (Input.GetKeyDown(KeyCode.E)) GetPlayer().EquipWrenchRpc();
                        if (Input.GetKeyDown(KeyCode.Q)) GetPlayer().EquipMedkitRpc();
                    }
                       
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
            if (tile && tile.Space != Player.Space && tile.canBeBoarded)
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
     
        crew.Faction.Value = 1;
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
        ScriptableBackground back = Resources.Load<ScriptableBackground>($"OBJ/SCRIPTABLES/BACKGROUNDS/{backTex}");
        Debug.Log($"Set initial background: {back} searched at OBJ/SCRIPTABLES/BACKGROUNDS/{backTex}");
        crew.SetCharacterBackground(back); //Must be called BEFORE INIT
        crew.NetworkObject.Spawn();
        crew.Init();
        crew.SetHomeDrifter(CO.co.PlayerMainDrifter);
        crew.RegisterPlayerOnLOCALCORpc();
        Player.EquipWeapon(0, back.Background_StartingWeapon);
        if (back.Background_StartingWeapon2) Player.EquipWeapon(1, back.Background_StartingWeapon2);
        //Player.EquipWeaponPrefab(0);

        foreach (FactionReputation rep in back.Background_ReputationEffect)
        {
            CO.co.Resource_Reputation[rep.Fac] = (CO.co.Resource_Reputation.ContainsKey(rep.Fac) ? CO.co.Resource_Reputation[rep.Fac] : 0) + rep.Amount;
        }

        CO.co.PlayerMainDrifter.Interior.AddCrew(crew);
    }
    public void SetCameraToPlayer()
    {
        if (CurrentControlMode == ControlModes.PLAYER) return;
        CurrentControlMode = ControlModes.PLAYER;
        CAM.cam.SetCameraMode(Player.transform, CAM.cam.GetPlayerZoom(), 8f, Mathf.Min(20f + Player.GetATT_COMMUNOPATHY(),29));
    }

    public void SetCameraToCommand()
    {
        if (CurrentControlMode == ControlModes.COMMAND) return;
        CurrentControlMode = ControlModes.COMMAND;
        CAM.cam.SetCameraMode(CO.co.PlayerMainDrifter.transform, CAM.cam.GetFarZoom(), 30f, 150f);
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
                if (Input.GetKeyDown(KeyCode.F) && !UI.ui.ChatUI.ChatScreen.activeSelf)
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
                        if (mod is Module)
                        {
                            if (((Module)mod).IsDisabledForever()) UI.ui.MainGameplayUI.SetInteractTex("[DESTROYED]", new Color(0.5f,0,0));
                            else UI.ui.MainGameplayUI.SetInteractTex("[DISABLED]", Color.red);
                        }
                    }
                    else
                    {
                        bool Pass = true;
                        if (mod is Module)
                        {
                            if (Player.Space != CO.co.PlayerMainDrifter.Space)
                            {
                                Pass = false;
                            }
                        }
                        if (Pass)
                        {
                            switch (mod.GetInteractableType())
                            {
                                case Module.ModuleTypes.WEAPON:
                                    ModuleWeapon weapon = mod as ModuleWeapon;
                                    string str;

                                    if (weapon.ReloadingCurrently.Value)
                                    {
                                        str = $"{weapon.ModuleTag}\nRELOADING...";
                                    }
                                    else
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
                                        UI.ui.OpenTalkScreenFancy(UI.ui.InventoryUI.gameObject);
                                    break;
                                case Module.ModuleTypes.MAPCOMMS:
                                    if (!CO.co.AreWeInDanger.Value)
                                    {
                                        if (CO_STORY.co.IsCommsActive())
                                        {
                                            UI.ui.MainGameplayUI.SetInteractTex("[F] OPEN COMMS", Color.cyan);
                                            if (Input.GetKeyDown(KeyCode.F))
                                            {
                                                UI.ui.OpenTalkScreenFancy(UI.ui.TalkUI.gameObject);
                                            }
                                        }
                                        else
                                        {
                                            UI.ui.MainGameplayUI.SetInteractTex("[F] OPEN MAP", Color.cyan);
                                            if (Input.GetKeyDown(KeyCode.F))
                                            {
                                                UI.ui.OpenTalkScreenFancy(UI.ui.MapUI.gameObject);
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
                }
            } else if ((Input.GetKeyDown(KeyCode.F) && !UI.ui.ChatUI.ChatScreen.activeSelf) || LeftModule())
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
        if (!IsOwner) return;
        UI.ui.FadeToBlack(1f,1f);
        UI.ui.SetCinematicTex(str, Color.green, 4f, 1f);
        CAM.cam.SetCameraCinematic(200f);
        UI.ui.SelectScreen(null);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShipTransportFadeInRpc()
    {
        if (!IsOwner) return;
        UI.ui.FadeFromBlack(2f);
        StartCoroutine(ArrivalAnimation());
    }

    bool arrivalAnimation = false;

    public bool IsArrivingAnimation()
    {
        return arrivalAnimation;
    }
    IEnumerator ArrivalAnimation()
    {
        arrivalAnimation = true;
        CurrentControlMode = ControlModes.NONE;
        CAM.cam.SetCameraMode(Vector3.zero, 150f, 150f, 150f);
        yield return new WaitForSeconds(3f);
        CurrentControlMode = ControlModes.NONE;
        SetCameraToPlayer();
        yield return new WaitForSeconds(1.5f);
        UI.ui.OpenTalkScreenFancy(UI.ui.TalkUI.gameObject);
        arrivalAnimation = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CinematicTexRpc(string str)
    {
        //Called on LOCALCO.local of the main server like a weirdo
        UI.ui.SetCinematicTex(str, Color.green, 4f, 1f);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void PanCameraRpc(Vector3 vec, float size, float dur)
    {
        if (!IsOwner) return;
        StartCoroutine(PanCameraNum(vec, size, dur));
    }

    IEnumerator PanCameraNum(Vector3 vec, float size, float dur)
    {
        CurrentControlMode = ControlModes.NONE;
        CAM.cam.SetCameraMode(vec, 150f, 150f, 150f);
        yield return new WaitForSeconds(dur);
        SetCameraToPlayer();
    }

    [Rpc(SendTo.NotOwner)]
    public void SetChatMessageToEveryoneElseRpc(string messageForOthers)
    {
        UI.ui.ChatUI.CreateChatMessage(messageForOthers);
    }
}
