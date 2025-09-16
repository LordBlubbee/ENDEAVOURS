using Unity.Netcode;
using UnityEngine;

public class LOCALCO : NetworkBehaviour
{
    public static LOCALCO local;
    private NetworkVariable<int> PlayerID = new NetworkVariable<int>();
    private CREW Player;
    public CREW GetPlayer()
    {
        //Works only locally
        return Player;
    }
    private void Start()
    {
        CO.co.RegisterLOCALCO(this);
        if (IsOwner) local = this;
    }

    private void Update()
    {
        //Controls
        if (GetPlayer())
        {
            Vector3 mov = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) mov += new Vector3(0, 1);
            if (Input.GetKey(KeyCode.S)) mov += new Vector3(0, -1);
            if (Input.GetKey(KeyCode.A)) mov += new Vector3(-1, 0);
            if (Input.GetKey(KeyCode.D)) mov += new Vector3(1, 0);
            GetPlayer().SetMoveInput(mov);
            GetPlayer().SetLookTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (Input.GetKeyDown(KeyCode.LeftShift)) GetPlayer().DashRpc();
            if (Input.GetMouseButtonDown(0)) GetPlayer().UseItem1Rpc();
            if (Input.GetMouseButtonDown(1)) GetPlayer().UseItem2Rpc();
        }
    }
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
    }

    [Rpc(SendTo.Server)]
    public void CreatePlayerRpc()
    {
        CREW crew = Instantiate(CO_SPAWNER.co.PlayerPrefab, Vector3.zero, Quaternion.identity);
        crew.NetworkObject.Spawn();
        Debug.Log($"We are player: {GetPlayerID()}");
        crew.PlayerController.Value = GetPlayerID();
        crew.Init();
        crew.RegisterPlayerOnLOCALCORpc();
    }
}
