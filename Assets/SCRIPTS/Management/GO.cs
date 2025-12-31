using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;

public class GO : MonoBehaviour
{
    //Global data singleton that contains all archived data information and is used to save and load game data!
    public static GO g;

    //SETTINGS
    [Header("Public Saves")]
    public float OST_Vol;
    public float VCX_Vol;
    public float SFX_Vol;
    public float mouseScrollSpeed;
    public float arrowScrollSpeed;
    public int screenShakeLevel;
    public int resolutionConfig;

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
        localColor = Color.white;
        screenShakeLevel = 2; //0 = none, 1 = cinematic, 2 = all
        resolutionConfig = 0; //0 = full screen, 1 = 2560x1600, 2 = 2560x1440, 3 = 1920x1200, 4 = 1920x1080, 5 = 1280x720
        mouseScrollSpeed = 0.2f;
        arrowScrollSpeed = 0.2f;
        currentSaveSlot = 0;
        OST_Vol = 0.8f;
        VCX_Vol = 0.8f;
        SFX_Vol = 0.8f;
        preferredHostControl = 0;
        preferredGameDifficulty = 0;
        preferredLobbyName = "";
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
        GO.g.CurrentSave = "/Mistworld" + slot;

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
    public void saveGame()
    {
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
    public string localUsername;
    public float localColorR;
    public float localColorG;
    public float localColorB;
    public float OST_Vol;
    public float VCX_Vol;
    public float SFX_Vol;

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
        GO.g.localUsername = localUsername;
        GO.g.localColor = new Color(localColorR, localColorG, localColorB);
        GO.g.currentSaveSlot = currentSaveSlot;
        GO.g.OST_Vol = OST_Vol;
        GO.g.VCX_Vol = VCX_Vol;
        GO.g.SFX_Vol = SFX_Vol;
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
        localUsername = GO.g.localUsername;
        localColorR = GO.g.localColor.r;
        localColorG = GO.g.localColor.g;
        localColorB = GO.g.localColor.b;
        currentSaveSlot = GO.g.currentSaveSlot;
        OST_Vol = GO.g.OST_Vol;
        VCX_Vol = GO.g.VCX_Vol;
        SFX_Vol = GO.g.SFX_Vol;
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

    int Resources_Materials;
    int Resources_Supplies;
    int Resources_Ammo;
    int Resources_Tech;

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
    }
    public dataStructure()
    {
        if (CO.co == null) return;
        //Save game
        saveTime = DateTime.Now;
        BiomeProgress = CO.co.BiomeProgress;

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

        BiomeType = CO.co.CurrentBiome.name;
        BiomeName = CO.co.CurrentBiome.BiomeName;
        DrifterType = CO.co.PlayerMainDrifterTypeID;
        foreach (ScriptableEquippable equip in CO.co.Drifter_Inventory)
        {
            DrifterInventoryItems.Add(equip.GetItemResourceIDFull());
        }
        foreach (Module mod in CO.co.PlayerMainDrifter.Interior.GetModules())
        {
            dataModule data = new dataModule();
            data.ModuleLink = mod.ShowAsModule.GetItemResourceIDFull();
            DrifterModules.Add(data);
        }
        foreach (CREW crew in CO.co.GetAlliedCrew())
        {
            if (crew.IsPlayer())
            {
                dataPlayer play = new dataPlayer();
                play.PlayerName = crew.CharacterName.Value.ToString();
                play.PlayerNameColor = crew.GetCharacterColor();

                play.PlayerBackground = crew.CharacterBackground.ResourcePath;
                play.PlayerXP = crew.XPPoints.Value;
                play.PlayerSkillPoints = crew.SkillPoints.Value;
                play.PlayerAttributes = crew.GetAttributes();
                play.PlayerWeapons = new string[]
                {
                    crew.EquippedWeapons[0].GetItemResourceIDFull(),
                     crew.EquippedWeapons[1].GetItemResourceIDFull(),
                      crew.EquippedWeapons[2].GetItemResourceIDFull()
                };
                play.PlayerArmor = crew.EquippedArmor.GetItemResourceIDFull();
                play.PlayerArtifacts = new string[]
              {
                    crew.EquippedArtifacts[0].GetItemResourceIDFull(),
                     crew.EquippedArtifacts[1].GetItemResourceIDFull(),
                      crew.EquippedArtifacts[2].GetItemResourceIDFull()
              };
                DrifterPlayers.Add(play);
            } else
            {
                dataNonPlayerCrew play = new dataNonPlayerCrew();
                play.PlayerName = crew.CharacterName.Value.ToString();
                play.CrewLink = crew.UnitLink;
                play.CrewLevel = crew.GetUnitUpgradeLevel();
                DrifterAICrew.Add(play);
            }
        }
        int i = 0;
        foreach (MapPoint equip in CO.co.GetMapPoints())
        {
            dataMapPoint point = new dataMapPoint();
            point.PointLink = equip.AssociatedPoint.GetResourceLink();
            point.Position = equip.transform.position;

            if (equip == CO.co.GetPlayerMapPoint()) OurMapPointPositionID = i;

            MapPointList.Add(point);
            i++;
        }
    }
}

[Serializable]
public class dataMapPoint
{
    public string PointLink;
    public Vector3 Position;
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
    public string CrewLink;
    public int CrewLevel;
}

[Serializable]
public class dataPlayer
{
    public string PlayerName;
    public Color PlayerNameColor;
    public string PlayerBackground;
    public int PlayerXP;
    public int PlayerSkillPoints;
    public int[] PlayerAttributes;

    public string[] PlayerWeapons;
    public string PlayerArmor;
    public string[] PlayerArtifacts;
}