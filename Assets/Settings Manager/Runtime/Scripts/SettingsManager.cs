using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public static SettingsData Settings { get; private set; }
    public static SettingsManager Instance { get; private set; } = null;

    private const string SettingsFileName = "settings.json";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        Instance = this;

        LoadOrCreateSettings();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string SettingsFilePath
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar + SettingsFileName;
        }
    }

    void LoadOrCreateSettings()
    {
        Settings = new SettingsData();

        // file already exists so load it
        if (File.Exists(SettingsFilePath))
        {
            // load the JSON content
            string settingsJSON = File.ReadAllText(SettingsFilePath);
            
            // parse the data into the settings
            JsonUtility.FromJsonOverwrite(settingsJSON, Settings);
        }

        Settings.Validate();
        Settings.ApplySettings();

        SaveAllChanges();
    }

    public void SaveAllChanges()
    {
        string settingsJSON = JsonUtility.ToJson(Settings, true);
        File.WriteAllText(SettingsFilePath, settingsJSON);
    }
}
