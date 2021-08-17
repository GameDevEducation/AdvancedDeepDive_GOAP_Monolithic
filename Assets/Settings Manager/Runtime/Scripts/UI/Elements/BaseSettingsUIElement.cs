using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class BaseSettingsUIElement : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] protected TextMeshProUGUI Name;
    [SerializeField] protected TextMeshProUGUI Description;
    [SerializeField] protected SettingUID UniqueID = SettingUID.Unknown;
    [SerializeField] protected string Category;
    [SerializeField] protected string SettingName;
    #pragma warning restore 0649

#if UNITY_EDITOR
    public virtual void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        UniqueID = (SettingUID)settingUIDIndex;
        Category = settingAttribute.Category;
        Name.text = SettingName = settingAttribute.DisplayName;
        Description.text = settingAttribute.HelpText;
    }
#endif // UNITY_EDITOR

    public virtual void Awake()
    {

    }

    public virtual void Start()
    {

    }

    public virtual void PopulateInitialValue()
    {
        throw new System.MissingMethodException("Settings must override the PopulateInitialValue method.");
    }
    
    public virtual void SynchroniseUIWithSettings()
    {
        throw new System.MissingMethodException("Settings must override the SynchroniseUIWithSettings method.");
    }

    public virtual void ResetToOriginalValue()
    {
        throw new System.MissingMethodException("Settings must override the ResetToOriginalValue method.");
    }
}
