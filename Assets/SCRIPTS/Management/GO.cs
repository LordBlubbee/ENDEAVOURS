using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GO : MonoBehaviour
{
    //Global data singleton that contains all archived data information and is used to save and load game data!
    public static GO g;

    //SETTINGS
    [Header("Public Saves")]
    public string SaveID;

    public float OST_Vol;
    public float VCX_Vol;
    public float SFX_Vol;
    public float mouseScrollSpeed;
    public float arrowScrollSpeed;
    public int screenShakeLevel;
    public int resolutionConfig;

    public bool enableTutorial;

    public int currentSaveSlot;
    public int preferredHostControl;
    public int preferredGameDifficulty;
    public string preferredLobbyName;

    public string localUsername;
    public Color localColor;

    //SAVING SYSTEM
    public string CurrentSave;

    private void Awake()
    {
        if (GO.g == null) { GO.g = this; } else { Destroy(this); return; }
        DontDestroyOnLoad(this);
        loadSettings();
    }
    public void firstSettings()
    {
        //Initialize important global preferences variables
        if (SaveID == "")
        {
            SaveID = "";
            for (int i = 0; i < 28; i++)
            {
                SaveID += UnityEngine.Random.Range(0, 10).ToString();
            }
            Debug.Log($"Created new settings and set SaveID to {SaveID}");
        }
        localColor = Color.white;
        screenShakeLevel = 2; //0 = none, 1 = cinematic, 2 = all
        resolutionConfig = 0; //0 = full screen, 1 = 2560x1600, 2 = 2560x1440, 3 = 1920x1200, 4 = 1920x1080, 5 = 1280x720
        mouseScrollSpeed = 0.2f;
        arrowScrollSpeed = 0.2f;
        currentSaveSlot = 0;
        enableTutorial = true;
        OST_Vol = 0.5f;
        VCX_Vol = 0.8f;
        SFX_Vol = 0.8f;
        preferredHostControl = 0;
        preferredGameDifficulty = 1;
        preferredLobbyName = "Lobby";
    }
    public void loadSettings()
    {
        string path = Application.persistentDataPath + "/MistworldPrefs";

        if (File.Exists(path))
        {
            Debug.Log("Loading preferences data at " + path);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dataSettings data = formatter.Deserialize(stream) as dataSettings; //Load dataStructure that had old GO variables
            data.loadSettings();

            stream.Close();
        }
        else
        {
            Debug.Log("No preferences data file found, generating save at " + path);
            firstSettings();
            saveSettings();
        }
    }
    public dataStructure loadSlot(int slot)
    {
        string path = Application.persistentDataPath + "/Mistworld" + slot;
        //GO.g.CurrentSave = "/Mistworld" + slot;

        if (File.Exists(path))
        {
            Debug.Log("Loading save at " + path);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dataStructure data = formatter.Deserialize(stream) as dataStructure; //Load dataStructure that had old GO variables

            stream.Close();
            return data;
        }
        return null;
    }

    public DateTime LastSaveTime = DateTime.MinValue;
    public void saveGame()
    {
        LastSaveTime = DateTime.Now;
        saveGame(currentSaveSlot);
    }
    public void saveGame(int slot)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/Mistworld" + slot;

        FileStream stream = new FileStream(path, FileMode.Create);

        dataStructure saveData = new dataStructure(); //Save dataStructure with copies GO variables

        formatter.Serialize(stream, saveData);
        stream.Close();
    }
    public void deleteGame(int slot)
    {
        string path = Application.persistentDataPath + "/Mistworld" + slot;

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    public void saveSettings()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/MistworldPrefs";

        FileStream stream = new FileStream(path, FileMode.Create);

        dataSettings saveData = new dataSettings(); //Save dataSettings with copies GO variables

        formatter.Serialize(stream, saveData);
        stream.Close();
    }
}

[Serializable]
public class dataSettings
{
    //SETTINGS
    public string SaveID;

    public string localUsername;
    public float localColorR;
    public float localColorG;
    public float localColorB;
    public float OST_Vol;
    public float VCX_Vol;
    public float SFX_Vol;

    public bool enableTutorial;

    public int currentSaveSlot;
    public float mouseScrollSpeed;
    public float arrowScrollSpeed;
    public int screenShakeLevel;
    public int resolutionConfig;
    public int preferredHostControl;
    public int preferredGameDifficulty;
    public string preferredLobbyName;
    public void loadSettings()
    {
        GO.g.SaveID = SaveID;
        GO.g.localUsername = localUsername;
        GO.g.localColor = new Color(localColorR, localColorG, localColorB);
        GO.g.currentSaveSlot = currentSaveSlot;
        GO.g.OST_Vol = OST_Vol;
        GO.g.VCX_Vol = VCX_Vol;
        GO.g.SFX_Vol = SFX_Vol;
        GO.g.enableTutorial = enableTutorial;
        GO.g.mouseScrollSpeed = mouseScrollSpeed;
        GO.g.arrowScrollSpeed = arrowScrollSpeed;
        GO.g.screenShakeLevel = screenShakeLevel;
        GO.g.resolutionConfig = resolutionConfig;
        GO.g.preferredHostControl = preferredHostControl;
        GO.g.preferredGameDifficulty = preferredGameDifficulty;
        GO.g.preferredLobbyName = preferredLobbyName;
        //Set GO variables to n. variables
    }
    public dataSettings()
    {
        //Set local variables to GO variables
        SaveID = GO.g.SaveID;
        localUsername = GO.g.localUsername;
        localColorR = GO.g.localColor.r;
        localColorG = GO.g.localColor.g;
        localColorB = GO.g.localColor.b;
        currentSaveSlot = GO.g.currentSaveSlot;
        OST_Vol = GO.g.OST_Vol;
        VCX_Vol = GO.g.VCX_Vol;
        SFX_Vol = GO.g.SFX_Vol;
        enableTutorial = GO.g.enableTutorial;
        mouseScrollSpeed = GO.g.mouseScrollSpeed;
        arrowScrollSpeed = GO.g.arrowScrollSpeed;
        screenShakeLevel = GO.g.screenShakeLevel;
        resolutionConfig = GO.g.resolutionConfig;
        preferredHostControl = GO.g.preferredHostControl;
        preferredGameDifficulty = GO.g.preferredGameDifficulty;
        preferredLobbyName = GO.g.preferredLobbyName;
    }
}

[Serializable]
public class dataStructure
{
    //Add save variables here
    public DateTime saveTime;
    public int BiomeProgress;
    public string BiomeName;
    public int SavedDifficulty;

    int Resources_Materials;
    int Resources_Supplies;
    int Resources_Ammo;
    int Resources_Tech;
    int Resources_TotalXP;
    float DrifterHull;

    List<int> FactionKeys = new();
    List<int> FactionValues = new();

    string DrifterType;
    string BiomeType;
    List<string> DrifterInventoryItems = new();
    List<dataPlayer> DrifterPlayers = new();
    List<dataNonPlayerCrew> DrifterAICrew = new();
    List<dataModule> DrifterModules = new();
    List<dataMapPoint> MapPointList = new();
    int OurMapPointPositionID;
    public void loadGame()
    {
        dataStructure n = this;

        CO.co.BiomeProgress.Value = n.BiomeProgress;
        CO.co.LoadDifficulty(n.SavedDifficulty);

        CO.co.Resource_Materials.Value = n.Resources_Materials;
        CO.co.Resource_Supplies.Value = n.Resources_Supplies;
        CO.co.Resource_Ammo.Value = n.Resources_Ammo;
        CO.co.Resource_Tech.Value = n.Resources_Tech;
        CO.co.Resource_TotalXP.Value = n.Resources_TotalXP;

        CO.co.Resource_Reputation = new();
        for (int i = 0; i < n.FactionKeys.Count; i++)
        {
            CO.co.Resource_Reputation.Add((CO.Faction)n.FactionKeys[i], n.FactionValues[i]);
        }
        CO.co.SetLoadedPlayers(n.DrifterPlayers);

        foreach (LOCALCO local in CO.co.GetLOCALCO())
        {
            string ID = local.PlayerSaveID.Value.ToString();
            foreach (dataPlayer pl in CO.co.GetLoadedPlayers())
            {
                if (pl.PlayerOwnerID.Equals(ID))
                {
                    local.AddLoadedCharacter(pl);
                    break;
                }
            }
        }

        CO.co.CurrentBiome = Resources.Load<ScriptableBiome>($"OBJ/SCRIPTABLES/BIOMES/{BiomeType}");

        CO.co.LoadMap(MapPointList);
        CO.co.PlayerMapPointID.Value = OurMapPointPositionID;
        Debug.Log($"We are point {OurMapPointPositionID} in a total of {CO.co.GetMapPoints().Count} points");

        CO_SPAWNER.co.SpawnLoadedPlayerShip(n.DrifterType, DrifterHull, DrifterModules);

        foreach (dataNonPlayerCrew Crew in DrifterAICrew)
        {
            CREW TypeOfCrew = Resources.Load<CREW>($"OBJ/CREW/{Crew.CrewLink}");
            CREW Spawned = CO_SPAWNER.co.SpawnUnitOnShip(TypeOfCrew, CO.co.PlayerMainDrifter);
            Spawned.CharacterName.Value = Crew.PlayerName;
            Spawned.CharacterNameColor.Value = new Vector3(Crew.localColorR, Crew.localColorG, Crew.localColorB);
            Spawned.AddUpgradeLevel(Crew.CrewLevel);
        }

        foreach (dataPlayer play in CO.co.GetLoadedPlayers())
        {
            Debug.Log($"Loading player entity {play.PlayerName}");
            ScriptableEquippable scr; 
           
            for (int i = 0; i < 3; i++)
            {
                if (play.PlayerWeapons[i] != "")
                {
                    scr = Resources.Load<ScriptableEquippable>(play.PlayerWeapons[i]);
                    CO.co.AddInventoryItem(scr);
                }
            }
            if (play.PlayerArmor != "")
            {
                scr = Resources.Load<ScriptableEquippable>(play.PlayerArmor);
                CO.co.AddInventoryItem(scr);
            }
            for (int i = 0; i < 3; i++)
            {
                if (play.PlayerArtifacts[i] != "")
                {
                    scr = Resources.Load<ScriptableEquippable>(play.PlayerArtifacts[i]);
                    CO.co.AddInventoryItem(scr);
                }
            }
        }
        foreach (string str in DrifterInventoryItems)
        {
            ScriptableEquippable scr = Resources.Load<ScriptableEquippable>(str);
            CO.co.AddInventoryItem(scr);
        }

        CO.co.StartLoadedGame();
    }
    public dataStructure()
    {
        if (CO.co == null) return;
        //Save game
        saveTime = DateTime.Now;
        BiomeProgress = CO.co.GetBiomeProgress();
        SavedDifficulty = CO.co.GetDifficulty();

        foreach (var kvp in CO.co.Resource_Reputation)
        {
            // Enum -> int
            FactionKeys.Add((int)kvp.Key);
            FactionValues.Add(kvp.Value);
        }

        Resources_Materials = CO.co.Resource_Materials.Value;
        Resources_Supplies = CO.co.Resource_Supplies.Value;
        Resources_Ammo = CO.co.Resource_Ammo.Value;
        Resources_Tech = CO.co.Resource_Tech.Value;
        Resources_TotalXP = CO.co.Resource_TotalXP.Value;
        DrifterHull = CO.co.PlayerMainDrifter.GetHealth();

        BiomeType = CO.co.CurrentBiome.name;
        BiomeName = CO.co.CurrentBiome.BiomeName;
        DrifterType = CO.co.PlayerMainDrifterTypeID;
        foreach (ScriptableEquippable equip in CO.co.Drifter_Inventory)
        {
            if (equip == null) continue;
            DrifterInventoryItems.Add(equip.GetItemResourceIDFull());
        }
        foreach (Module mod in CO.co.PlayerMainDrifter.Interior.GetModules())
        {
            if (mod == null) continue;
            if (!mod.ShowAsModule) continue;
            dataModule data = new dataModule();
            data.ModuleLink = mod.ShowAsModule.GetItemResourceIDFull();
            data.ModuleLevel = mod.ModuleLevel.Value;
            DrifterModules.Add(data);
        }
        foreach (CREW crew in CO.co.GetAlliedCrew())
        {
            if (crew.IsPlayer())
            {
                CO.co.SavePlayer(crew);
                //DrifterPlayers.Add(play);
            } else
            {
                dataNonPlayerCrew play = new dataNonPlayerCrew();
                play.PlayerName = crew.CharacterName.Value.ToString();
                play.localColorR = crew.CharacterNameColor.Value.x;
                play.localColorG = crew.CharacterNameColor.Value.y;
                play.localColorB = crew.CharacterNameColor.Value.z;
                play.CrewLink = crew.UnitLink;
                play.CrewLevel = crew.GetUnitUpgradeLevel();
                DrifterAICrew.Add(play);
            }
        }
        DrifterPlayers = CO.co.GetLoadedPlayers();
        int i = 0;
        MapPoint WeAreHere = CO.co.GetPlayerMapPoint();
        foreach (MapPoint equip in CO.co.GetMapPoints())
        {
            dataMapPoint point = new dataMapPoint();
            point.PointLink = equip.AssociatedPoint.GetResourceLink();
            point.PositionX = equip.transform.position.x;
            point.PositionY = equip.transform.position.y;

            if (equip == WeAreHere) OurMapPointPositionID = i;

            MapPointList.Add(point);
            i++;
        }
    }
}

[Serializable]
public class dataMapPoint
{
    public string PointLink;
    public float PositionX;
    public float PositionY;

    public Vector3 GetPosition()
    {
        return new Vector3(PositionX, PositionY, 0);
    }
}

[Serializable]
public class dataModule
{
    public string ModuleLink;
    public int ModuleLevel;
}

[Serializable]
public class dataNonPlayerCrew
{
    public string PlayerName;
    public float localColorR;
    public float localColorG;
    public float localColorB;

    public Color getColor()
    {
        return new Color(localColorR, localColorG, localColorB);
    }

    public string CrewLink;
    public int CrewLevel;
}

[Serializable]
public class dataPlayer
{
    public string PlayerOwnerID;

    public string PlayerName;
    public float localColorR;
    public float localColorG;
    public float localColorB;

    public Color getColor()
    {
        return new Color(localColorR, localColorG, localColorB);
    }

    public string PlayerBackground;
    public int PlayerXP;
    public int PlayerSkillPoints;
    public int PlayerXPTotal;
    public int[] PlayerAttributes;

    public string[] PlayerWeapons;
    public string PlayerArmor;
    public string[] PlayerArtifacts;
}