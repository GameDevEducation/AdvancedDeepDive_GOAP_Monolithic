using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIElement_Float : BaseSettingsUIElement
{
    #pragma warning disable 0649
    [SerializeField] Slider ValueSlider;
    [SerializeField] float DefaultValue;
    #pragma warning restore 0649

    private float InitialValue = float.MaxValue;

#if UNITY_EDITOR
    public override void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        base.BindToSetting(settingUIDIndex, settingAttribute);

        FloatSetting typeSpecificConfig = settingAttribute as FloatSetting;
        ValueSlider.minValue = typeSpecificConfig.MinValue;
        ValueSlider.maxValue = typeSpecificConfig.MaxValue;
        DefaultValue = typeSpecificConfig.DefaultValue;
        ValueSlider.SetValueWithoutNotify(typeSpecificConfig.DefaultValue);        
    }
#endif // UNITY_EDITOR

    public override void PopulateInitialValue()
    {
        InitialValue = SettingsBinder.GetFloat(UniqueID, SettingsManager.Settings);

        Name.text = SettingName + " [" + InitialValue.ToString("0.00") + "]";
    }

    public override void SynchroniseUIWithSettings()
    {
        float currentValue = SettingsBinder.GetFloat(UniqueID, SettingsManager.Settings);
        ValueSlider.SetValueWithoutNotify(currentValue);

        Name.text = SettingName + " [" + currentValue.ToString("0.00") + "]";
    }

    public override void ResetToOriginalValue()
    {
        SettingsBinder.SetFloat(UniqueID, InitialValue, SettingsManager.Settings);
        ValueSlider.SetValueWithoutNotify(InitialValue);

        Name.text = SettingName + " [" + InitialValue.ToString("0.00") + "]";
    }

    public void OnValueChanged(float newValue)
    {
        SettingsBinder.SetFloat(UniqueID, newValue, SettingsManager.Settings);

        Name.text = SettingName + " [" + newValue.ToString("0.00") + "]";
    }    
}
