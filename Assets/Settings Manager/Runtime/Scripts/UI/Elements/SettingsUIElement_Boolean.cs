using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIElement_Boolean : BaseSettingsUIElement
{
    #pragma warning disable 0649
    [SerializeField] Toggle ToggleBox;
    [SerializeField] bool DefaultValue;
    #pragma warning restore 0649

    private bool InitialValue = false;

#if UNITY_EDITOR
    public override void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        base.BindToSetting(settingUIDIndex, settingAttribute);

        BooleanSetting typeSpecificConfig = settingAttribute as BooleanSetting;
        DefaultValue = typeSpecificConfig.DefaultValue;
        ToggleBox.SetIsOnWithoutNotify(typeSpecificConfig.DefaultValue);
    }
#endif // UNITY_EDITOR

    public override void PopulateInitialValue()
    {
        InitialValue = SettingsBinder.GetBoolean(UniqueID, SettingsManager.Settings);
    }

    public override void SynchroniseUIWithSettings()
    {
        ToggleBox.SetIsOnWithoutNotify(SettingsBinder.GetBoolean(UniqueID, SettingsManager.Settings));
    }

    public override void ResetToOriginalValue()
    {
        SettingsBinder.SetBoolean(UniqueID, InitialValue, SettingsManager.Settings);
        ToggleBox.SetIsOnWithoutNotify(InitialValue);
    }

    public void OnValueChanged(bool newValue)
    {
        SettingsBinder.SetBoolean(UniqueID, newValue, SettingsManager.Settings);
    }
}
