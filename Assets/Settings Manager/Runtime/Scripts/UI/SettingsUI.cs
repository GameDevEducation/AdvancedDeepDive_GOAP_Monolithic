using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif // UNITY_EDITOR

[System.Serializable]
public class SettingsElementList
{
    public List<BaseSettingsUIElement> Elements = new List<BaseSettingsUIElement>();

    public void AddElement(BaseSettingsUIElement element)
    {
        Elements.Add(element);
    }
}

[System.Serializable]
public class SettingsHierarchyData
{
    public List<string> Categories = new List<string>();
    public List<SettingsElementList> ElementLists = new List<SettingsElementList>();
    [System.NonSerialized] private Dictionary<string, List<BaseSettingsUIElement>> _Hierarchy = null;

    public bool HasCategory(string name)
    {
        return Categories != null && Categories.Contains(name);
    }

    public void AddCategory(string name)
    {
        Categories.Add(name);
    }

    public void AddElement(string categoryName, BaseSettingsUIElement element)
    {
        int categoryIndex = Categories.IndexOf(categoryName);

        if (categoryIndex < 0)
            throw new System.InvalidOperationException("Unable to find category " + categoryName);

        while (ElementLists.Count <= categoryIndex)
            ElementLists.Add(new SettingsElementList());

        ElementLists[categoryIndex].AddElement(element);
    }

    public Dictionary<string, List<BaseSettingsUIElement>> Hierarchy
    {
        get
        {
            if (_Hierarchy == null)
            {
                _Hierarchy = new Dictionary<string, List<BaseSettingsUIElement>>();

                // build the dictionary
                for (int categoryIndex = 0; categoryIndex < Categories.Count; ++categoryIndex)
                {
                    string category = Categories[categoryIndex];
                    _Hierarchy[category] = new List<BaseSettingsUIElement>();

                    foreach(var element in ElementLists[categoryIndex].Elements)
                    {
                        _Hierarchy[category].Add(element);
                    }
                }
            }

            return _Hierarchy;
        }
    }
}

public class SettingsUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject CategoryContainer;
    [SerializeField] private GameObject SettingsElementsContainer;
    [SerializeField] [HideInInspector] private SettingsHierarchyData SettingsHierarchy;
    [SerializeField] private string DefaultCategory;

    private SettingsUI_Category[] CategoryElements;

    public UnityEvent OnSaveAndClose;
    public UnityEvent OnCancelAndClose;

#if UNITY_EDITOR
    [SerializeField] private GameObject Prefab_Category;
    [SerializeField] private GameObject Prefab_Integer;
    [SerializeField] private GameObject Prefab_Float;
    [SerializeField] private GameObject Prefab_Boolean;
    [SerializeField] private GameObject Prefab_String;
    [SerializeField] private GameObject Prefab_GraphicsQualityAndResolution;
#endif // UNITY_EDITOR
#pragma warning restore 0649

    public static SettingsUI Instance { get; private set; } = null;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CategoryElements = CategoryContainer.GetComponentsInChildren<SettingsUI_Category>();
        
        ActivateCategory(DefaultCategory);
    }

    void OnEnable()
    {
        foreach (string category in SettingsHierarchy.Hierarchy.Keys)
        {
            foreach (var settingsElement in SettingsHierarchy.Hierarchy[category])
            {
                settingsElement.PopulateInitialValue();
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Input_SaveRequested()
    {
        SettingsManager.Instance.SaveAllChanges();

        OnSaveAndClose?.Invoke();
    }

    public void Input_ResetRequested()
    {
        // reset to the original value
        foreach(string category in SettingsHierarchy.Hierarchy.Keys)
        {
            foreach(var settingsElement in SettingsHierarchy.Hierarchy[category])
            {
                settingsElement.ResetToOriginalValue();
            }
        }
    }

    public void Input_AbandonChangesRequested()
    {
        // reset the values
        Input_ResetRequested();

        // send exit notification
        OnCancelAndClose?.Invoke();
    }

    public void ActivateCategory(string categoryName)
    {
        // update the category elements
        foreach(SettingsUI_Category category in CategoryElements)
        {
            category.SetCategoryIsShown(category.Name == categoryName);
        }

        // update which settings are visible
        foreach(string category in SettingsHierarchy.Hierarchy.Keys)
        {
            foreach(var settingsElement in SettingsHierarchy.Hierarchy[category])
            {
                settingsElement.gameObject.SetActive(category == categoryName);

                settingsElement.SynchroniseUIWithSettings();
            }
        }
    }

    public void ForceRefreshCategory(string category)
    {
        foreach (var settingsElement in SettingsHierarchy.Hierarchy[category])
        {
            settingsElement.SynchroniseUIWithSettings();
        }
    }

#if UNITY_EDITOR
    List<string> SettingUIDs;

    List<string> Getters_Boolean;
    List<string> Getters_Integer;
    List<string> Getters_Float;
    List<string> Getters_String;
    List<string> Getters_GraphicsQualityAndResolution;

    List<string> Setters_Boolean;
    List<string> Setters_Integer;
    List<string> Setters_Float;
    List<string> Setters_String;
    List<string> Setters_GraphicsQualityAndResolution;

    public void GenerateUI()
    {
        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Generate UI");

        // clear any existing data in preparation for the gen cycle
        GenerateUI_Reset();

        // process the top level fields
        FieldInfo[] topLevelFields = typeof(SettingsData).GetFields();
        for (int tlIndex = 0; tlIndex < topLevelFields.Length; ++tlIndex)
        {
            GenerateUI_ProcessClass(topLevelFields[tlIndex].Name, topLevelFields[tlIndex].FieldType);
        }

        // process the top level properties
        PropertyInfo[] topLevelProperties = typeof(SettingsData).GetProperties();
        for (int tlIndex = 0; tlIndex < topLevelProperties.Length; ++tlIndex)
        {
            GenerateUI_ProcessClass(topLevelProperties[tlIndex].Name, topLevelProperties[tlIndex].PropertyType);
        }

        GenerateUI_UpdateSettingsBinder();

        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void ClearUI()
    {
        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Clear UI");

        // clear any existing data in preparation for the gen cycle
        GenerateUI_Reset();

        EditorSceneManager.MarkSceneDirty(gameObject.scene);        
    }

    void GenerateUI_Reset()
    {
        DefaultCategory = null;

        SettingUIDs = new List<string>();

        Getters_Boolean                         = new List<string>();
        Getters_Integer                         = new List<string>();
        Getters_Float                           = new List<string>();
        Getters_String                          = new List<string>();
        Getters_GraphicsQualityAndResolution    = new List<string>();

        Setters_Boolean                         = new List<string>();
        Setters_Integer                         = new List<string>();
        Setters_Float                           = new List<string>();
        Setters_String                          = new List<string>();
        Setters_GraphicsQualityAndResolution    = new List<string>();

        SettingsHierarchy = new SettingsHierarchyData();   

        // build the list of objects to clear
        List<GameObject> elementsToDestroy = new List<GameObject>();
        for (int index = 0; index < CategoryContainer.transform.childCount; ++index)
            elementsToDestroy.Add(CategoryContainer.transform.GetChild(index).gameObject);
        for (int index = 0; index < SettingsElementsContainer.transform.childCount; ++index)
            elementsToDestroy.Add(SettingsElementsContainer.transform.GetChild(index).gameObject);

        // destroy the objects
        foreach(var element in elementsToDestroy)
        {
            Editor.DestroyImmediate(element);
        }
    }

    void GenerateUI_ProcessClass(string parentName, System.Type containerType)
    {
        FieldInfo[] allFields = containerType.GetFields();
        for (int fieldIndex = 0; fieldIndex < allFields.Length; ++fieldIndex)
        {
            FieldInfo candidateField = allFields[fieldIndex];

            ConfigurableSetting settingAttribute = candidateField.GetCustomAttribute<ConfigurableSetting>();
            if (settingAttribute == null)
                continue;

            GenerateUI_BindVariable(parentName, candidateField.Name, settingAttribute);
        }

        PropertyInfo[] allProperties = containerType.GetProperties();
        for (int propertyIndex = 0; propertyIndex < allProperties.Length; ++propertyIndex)
        {
            PropertyInfo candidateProperty = allProperties[propertyIndex];

            ConfigurableSetting settingAttribute = candidateProperty.GetCustomAttribute<ConfigurableSetting>();
            if (settingAttribute == null)
                continue;

            GenerateUI_BindVariable(parentName, candidateProperty.Name, settingAttribute);
        }        
    }

    void GenerateUI_BindVariable(string parentName, string variableName, ConfigurableSetting settingAttribute)
    {
        // create and add the UID for the setting
        string settingUID = AddSettingUID(parentName, variableName);
        int settingUIDIndex = SettingUIDs.Count;

        // instantiate the UI
        GenerateUI_InstantiatePrefab(settingUIDIndex, settingAttribute);

        // generate the getter
        GenerateUI_AddGetter(parentName, variableName, settingUID, settingAttribute);

        // generate the setter
        GenerateUI_AddSetter(parentName, variableName, settingUID, settingAttribute);
    }

    void GenerateUI_InstantiatePrefab(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        // add in the category if needed
        if (!SettingsHierarchy.HasCategory(settingAttribute.Category))
        {
            SettingsHierarchy.AddCategory(settingAttribute.Category);

            // instantiate the category
            GameObject categoryElement = Instantiate(Prefab_Category, CategoryContainer.transform);
            SettingsUI_Category categoryScript = categoryElement.GetComponent<SettingsUI_Category>();

            categoryScript.BindCategory(settingAttribute.Category, ActivateCategory);

            if (DefaultCategory == null)
                DefaultCategory = settingAttribute.Category;
        }

        // spawn the appropriate prefab
        GameObject settingsElement = null;
        if (settingAttribute is IntegerSetting)
            settingsElement = Instantiate(Prefab_Integer, SettingsElementsContainer.transform);
        else if (settingAttribute is FloatSetting)
            settingsElement = Instantiate(Prefab_Float, SettingsElementsContainer.transform);
        else if (settingAttribute is BooleanSetting)
            settingsElement = Instantiate(Prefab_Boolean, SettingsElementsContainer.transform);
        else if (settingAttribute is StringSetting)
            settingsElement = Instantiate(Prefab_String, SettingsElementsContainer.transform);
        else if (settingAttribute is GraphicsQualityAndResolutionSetting)
            settingsElement = Instantiate(Prefab_GraphicsQualityAndResolution, SettingsElementsContainer.transform);

        if (settingsElement == null)
            return;

        // retrieve and store the settings
        BaseSettingsUIElement settingsScript = settingsElement.GetComponent<BaseSettingsUIElement>();
        SettingsHierarchy.AddElement(settingAttribute.Category, settingsScript);

        // bind the settings script
        settingsScript.BindToSetting(settingUIDIndex, settingAttribute);
    }

    string AddSettingUID(string parentName, string variableName)
    {
        string settingUID = "Setting_" + (SettingUIDs.Count + 1).ToString() + "_" + parentName + "_" + variableName;

        SettingUIDs.Add(settingUID);

        return settingUID;
    }

    void GenerateUI_AddGetter(string parentName, string fieldName, string settingUID, ConfigurableSetting settingAttribute)
    {
        string getterCode = "";

        if (settingAttribute is GraphicsQualityAndResolutionSetting)
        {
            getterCode += "            case SettingUID." + settingUID + ":" + System.Environment.NewLine;
            getterCode += "                return settings." + parentName + "." + fieldName + ";" + System.Environment.NewLine;

            Getters_GraphicsQualityAndResolution.Add(getterCode);
        }
        else
        {
            getterCode += "            case SettingUID." + settingUID + ":" + System.Environment.NewLine;
            getterCode += "                return settings." + parentName + "." + fieldName + ";" + System.Environment.NewLine;

            if (settingAttribute is IntegerSetting)
                Getters_Integer.Add(getterCode);
            else if (settingAttribute is FloatSetting)
                Getters_Float.Add(getterCode);
            else if (settingAttribute is BooleanSetting)
                Getters_Boolean.Add(getterCode);
            else if (settingAttribute is StringSetting)
                Getters_String.Add(getterCode);
        }
    }

    void GenerateUI_AddSetter(string parentName, string fieldName, string settingUID, ConfigurableSetting settingAttribute)
    {
        string setterCode = "";

        if (settingAttribute is GraphicsQualityAndResolutionSetting)
        {
            setterCode += "            case SettingUID." + settingUID + ":" + System.Environment.NewLine;
            setterCode += "                settings." + parentName + "." + fieldName + "." + "UpdateFrom(newValue);" + System.Environment.NewLine;

            if (!string.IsNullOrEmpty(settingAttribute.ApplyMethod))
                setterCode += "                settings." + parentName + "." + settingAttribute.ApplyMethod + "();" + System.Environment.NewLine;
            if (settingAttribute.ForceRefreshOnChange)
                setterCode += "                SettingsUI.Instance.ForceRefreshCategory(\"" + settingAttribute.Category + "\");" + System.Environment.NewLine;

            setterCode += "                return;"+ System.Environment.NewLine;

            Setters_GraphicsQualityAndResolution.Add(setterCode);
        }
        else
        {
            setterCode += "            case SettingUID." + settingUID + ":" + System.Environment.NewLine;
            setterCode += "                settings." + parentName + "." + fieldName + " = newValue;" + System.Environment.NewLine;

            if (!string.IsNullOrEmpty(settingAttribute.ApplyMethod))
                setterCode += "                settings." + parentName + "." + settingAttribute.ApplyMethod + "();" + System.Environment.NewLine;

            if (settingAttribute.ForceRefreshOnChange)
                setterCode += "                SettingsUI.Instance.ForceRefreshCategory(\"" + settingAttribute.Category + "\");" + System.Environment.NewLine;

            setterCode += "                return;"+ System.Environment.NewLine;

            if (settingAttribute is IntegerSetting)
                Setters_Integer.Add(setterCode);
            else if (settingAttribute is FloatSetting)
                Setters_Float.Add(setterCode);
            else if (settingAttribute is BooleanSetting)
                Setters_Boolean.Add(setterCode);
            else if (settingAttribute is StringSetting)
                Setters_String.Add(setterCode);
        }
    }

    void GenerateUI_UpdateSettingsBinder()
    {
        string code = "";

        code += "public enum SettingUID" + System.Environment.NewLine;
        code += "{" + System.Environment.NewLine;
        code += "    Unknown," + System.Environment.NewLine;

        foreach(var uid in SettingUIDs) { code += "    " + uid + "," + System.Environment.NewLine;}

        code += "    NumSettings" + System.Environment.NewLine;
        code += "}" + System.Environment.NewLine;

        code += "static public class SettingsBinder" + System.Environment.NewLine;
        code += "{" + System.Environment.NewLine;
        code += "    public static string GetString(SettingUID uniqueID, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach (var getter in Getters_String) { code += getter + System.Environment.NewLine; }

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static void SetString(SettingUID uniqueID, string newValue, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach (var setter in Setters_String) { code += setter + System.Environment.NewLine; }

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static bool GetBoolean(SettingUID uniqueID, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;
        
        foreach(var getter in Getters_Boolean) { code += getter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static void SetBoolean(SettingUID uniqueID, bool newValue, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var setter in Setters_Boolean) { code += setter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static int GetInteger(SettingUID uniqueID, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var getter in Getters_Integer) { code += getter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static void SetInteger(SettingUID uniqueID, int newValue, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var setter in Setters_Integer) { code += setter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static float GetFloat(SettingUID uniqueID, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var getter in Getters_Float) { code += getter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static void SetFloat(SettingUID uniqueID, float newValue, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var setter in Setters_Float) { code += setter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static GraphicsQualityAndResolution GetGraphicsQualityAndResolution(SettingUID uniqueID, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var getter in Getters_GraphicsQualityAndResolution) { code += getter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;

        code += "    public static void SetGraphicsQualityAndResolution(SettingUID uniqueID, GraphicsQualityAndResolution newValue, SettingsData settings)" + System.Environment.NewLine;
        code += "    {" + System.Environment.NewLine;
        code += "        switch(uniqueID)" + System.Environment.NewLine;
        code += "        {" + System.Environment.NewLine;

        foreach(var setter in Setters_GraphicsQualityAndResolution) { code += setter + System.Environment.NewLine;}

        code += "            default:" + System.Environment.NewLine;
        code += "                break;" + System.Environment.NewLine;
        code += "        }" + System.Environment.NewLine;
        code += "        throw new System.ArgumentException(\"Unable to find setting \" + uniqueID + \". You may need to regenerate the Settings UI.\");" + System.Environment.NewLine;
        code += "    }" + System.Environment.NewLine;
        code += "}" + System.Environment.NewLine;

        // attempt to find the settings binder
        string[] foundAssets = AssetDatabase.FindAssets("SettingsBinder");
        if (foundAssets.Length != 1)
        {
            throw new System.IO.FileNotFoundException("Could not find one intance of the SettingsBinder.cs. Found " + foundAssets.Length);
        }

        // update the settings script
        string settingsBinderPath = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
        System.IO.File.WriteAllText(settingsBinderPath, code);
    }
#endif // UNITY_EDITOR
}
