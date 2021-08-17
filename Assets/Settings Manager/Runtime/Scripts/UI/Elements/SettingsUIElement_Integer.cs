using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIElement_Integer : BaseSettingsUIElement
{
    #pragma warning disable 0649
    [SerializeField] Slider ValueSlider;
    [SerializeField] int DefaultValue;
    #pragma warning restore 0649

    private int InitialValue = int.MaxValue;

#if UNITY_EDITOR
    public override void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        base.BindToSetting(settingUIDIndex, settingAttribute);

        IntegerSetting typeSpecificConfig = settingAttribute as IntegerSetting;
        ValueSlider.minValue = typeSpecificConfig.MinValue;
        ValueSlider.maxValue = typeSpecificConfig.MaxValue;
        DefaultValue = typeSpecificConfig.DefaultValue;
        ValueSlider.SetValueWithoutNotify(typeSpecificConfig.DefaultValue);
    }
#endif // UNITY_EDITOR

    public override void PopulateInitialValue()
    {
        InitialValue = SettingsBinder.GetInteger(UniqueID, SettingsManager.Settings);

        Name.text = SettingName + " [" + InitialValue + "]";
    }

    public override void SynchroniseUIWithSettings()
    {
        int currentValue = SettingsBinder.GetInteger(UniqueID, SettingsManager.Settings);
        ValueSlider.SetValueWithoutNotify(currentValue);

        Name.text = SettingName + " [" + currentValue + "]";
    }

    public override void ResetToOriginalValue()
    {
        SettingsBinder.SetInteger(UniqueID, InitialValue, SettingsManager.Settings);
        ValueSlider.SetValueWithoutNotify(InitialValue);

        Name.text = SettingName + " [" + InitialValue + "]";
    }

    public void OnValueChanged(float newValue)
    {
        SettingsBinder.SetInteger(UniqueID, (int)newValue, SettingsManager.Settings);

        Name.text = SettingName + " [" + (int)newValue + "]";
    }
}
