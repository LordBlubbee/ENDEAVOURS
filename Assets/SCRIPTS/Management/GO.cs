using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    public string localUsername;
    public Color localColor;

    //SAVING SYSTEM
    public string CurrentSave;
    public int ZoneProgress;
    public List<savedObject> SavedObjects;
    private void Awake()
    {
        if (GO.g == null) { GO.g = this; } else { Destroy(this); return; }
        DontDestroyOnLoad(this);
        loadSettings();
    }
    public void firstSave()
    {
        //Initialize important variables
        SavedObjects = new List<savedObject>();
        ZoneProgress = 1;
    }
    public void firstSettings()
    {
        //Initialize important global preferences variables
        localColor = Color.white;
        screenShakeLevel = 2; //0 = none, 1 = cinematic, 2 = all
        resolutionConfig = 0; //0 = full screen, 1 = 2560x1600, 2 = 2560x1440, 3 = 1920x1200, 4 = 1920x1080, 5 = 1280x720
        mouseScrollSpeed = 0.2f;
        arrowScrollSpeed = 0.2f;
        OST_Vol = 0.8f;
        VCX_Vol = 0.8f;
        SFX_Vol = 0.8f;
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
    public void loadSlot(int slot, bool overwrite)
    {
        string path = Application.persistentDataPath + "/Mistworld" + slot;
        GO.g.CurrentSave = "/Mistworld" + slot;

        if (File.Exists(path) && !overwrite)
        {
            Debug.Log("Loading save at " + path);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dataStructure data = formatter.Deserialize(stream) as dataStructure; //Load dataStructure that had old GO variables
            data.loadData();

            stream.Close();
        }
        else
        {
            Debug.Log("Creating new save at " + path);
            firstSave();
            saveGame();
        }
    }
    public DateTime getSlotTime(string slot)
    {
        string path = Application.persistentDataPath + slot;
        DateTime tim = DateTime.MinValue;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dataStructure data = formatter.Deserialize(stream) as dataStructure; //Load dataStructure that had old GO variables
            tim = data.getSaveTime();

            stream.Close();
        }
        return tim;
    }
    public int getSlotDay(string slot)
    {
        string path = Application.persistentDataPath + slot;
        int tim = 1;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dataStructure data = formatter.Deserialize(stream) as dataStructure; //Load dataStructure that had old GO variables
            tim = data.getDay();

            stream.Close();
        }
        return tim;
    }
    public void saveGame()
    {
        saveGame(CurrentSave);
    }
    public void saveGame(string slot)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + slot;

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
    public float mouseScrollSpeed;
    public float arrowScrollSpeed;
    public int screenShakeLevel;
    public int resolutionConfig;

    public void loadSettings()
    {
        GO.g.localUsername = localUsername;
        GO.g.localColor = new Color(localColorR, localColorG, localColorB);
        GO.g.OST_Vol = OST_Vol;
        GO.g.VCX_Vol = VCX_Vol;
        GO.g.SFX_Vol = SFX_Vol;
        GO.g.mouseScrollSpeed = mouseScrollSpeed;
        GO.g.arrowScrollSpeed = arrowScrollSpeed;
        GO.g.screenShakeLevel = screenShakeLevel;
        GO.g.resolutionConfig = resolutionConfig;
        //Set GO variables to n. variables
    }
    public dataSettings()
    {
        //Set local variables to GO variables
        localUsername = GO.g.localUsername;
        localColorR = GO.g.localColor.r;
        localColorG = GO.g.localColor.g;
        localColorB = GO.g.localColor.b;
        OST_Vol = GO.g.OST_Vol;
        VCX_Vol = GO.g.VCX_Vol;
        SFX_Vol = GO.g.SFX_Vol;
        mouseScrollSpeed = GO.g.mouseScrollSpeed;
        arrowScrollSpeed = GO.g.arrowScrollSpeed;
        screenShakeLevel = GO.g.screenShakeLevel;
        resolutionConfig = GO.g.resolutionConfig;
    }
}

[Serializable]
public class dataStructure
{
    //Add save variables here
    public DateTime saveTime;

    public int AdventureDay;
    public List<savedObject> SavedObjects;
    public void loadData()
    {
        dataStructure n = this;
        //Set GO variables to n. variables
        GO.g.ZoneProgress = AdventureDay;
        GO.g.SavedObjects = n.SavedObjects;

        //Compensate for new content
    }

    public DateTime getSaveTime()
    {
        dataStructure n = this;
        return n.saveTime;
    }
    public int getDay()
    {
        dataStructure n = this;
        return n.AdventureDay;
    }
    public string getSlot(int i)
    {
        return "/Conquest" + i;
    }
    public dataStructure()
    {
        AdventureDay = GO.g.ZoneProgress;
        SavedObjects = GO.g.SavedObjects;
        //Set local variables to GO variables
    }
}
[Serializable]

public class savedObject
{
    public string ob;
    public float posx;
    public float posy;
    public float rotz;
    public Dictionary<string, string> SavedStrings = new Dictionary<string, string>();
    public Dictionary<string, int> SavedIntegers = new Dictionary<string, int>();
    public Dictionary<string, float> SavedFloats = new Dictionary<string, float>();
    public savedObject(GameObject tar, string obref)
    {
        ob = obref;
        posx = tar.transform.position.x;
        posy = tar.transform.position.y;
        rotz = tar.transform.eulerAngles.z;
    }
}
public interface iSaveable
{
    Transform transform { get; }
    GameObject gameObject { get; }

    public string getPrefabLink();
    public void OnLoad(savedObject data);
    public savedObject OnSave(savedObject data);
}