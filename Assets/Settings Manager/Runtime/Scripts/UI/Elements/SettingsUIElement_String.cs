using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIElement_String : BaseSettingsUIElement
{
#pragma warning disable 0649
    [SerializeField] TMP_InputField TextInput;
    [SerializeField] string DefaultValue;
#pragma warning restore 0649

    private string InitialValue = string.Empty;

#if UNITY_EDITOR
    public override void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        base.BindToSetting(settingUIDIndex, settingAttribute);

        StringSetting typeSpecificConfig = settingAttribute as StringSetting;
        DefaultValue = typeSpecificConfig.DefaultValue;
        TextInput.SetTextWithoutNotify(typeSpecificConfig.DefaultValue);

        Name.text = SettingName;
    }
#endif // UNITY_EDITOR

    public override void PopulateInitialValue()
    {
        InitialValue = SettingsBinder.GetString(UniqueID, SettingsManager.Settings);
    }

    public override void SynchroniseUIWithSettings()
    {
        TextInput.SetTextWithoutNotify(SettingsBinder.GetString(UniqueID, SettingsManager.Settings));
    }

    public override void ResetToOriginalValue()
    {
        SettingsBinder.SetString(UniqueID, InitialValue, SettingsManager.Settings);
        TextInput.SetTextWithoutNotify(InitialValue);
    }

    public void OnValueChanged(string newValue)
    {
        SettingsBinder.SetString(UniqueID, newValue, SettingsManager.Settings);
    }
}
 