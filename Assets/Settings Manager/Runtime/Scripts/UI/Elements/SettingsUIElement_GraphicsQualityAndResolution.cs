using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIElement_GraphicsQualityAndResolution : BaseSettingsUIElement
{
    #pragma warning disable 0649
    [SerializeField] TMP_Dropdown ResolutionSelector;
    [SerializeField] TMP_Dropdown QualitySelector;
    #pragma warning restore 0649

    private List<string> QualityOptions;

    private List<string> ResolutionOptions = new List<string>();
    private List<Resolution> Resolutions = new List<Resolution>();

    private int InitialQualityLevel = -1;
    private int InitialResolution = -1;
    private bool InitialIsFullscreen = false;
    private GraphicsQualityAndResolution InitialGraphicsQualityAndResolution = null;
    private GraphicsQualityAndResolution CurrentGraphicsQualityAndResolution = null;

#if UNITY_EDITOR
    public override void BindToSetting(int settingUIDIndex, ConfigurableSetting settingAttribute)
    {
        base.BindToSetting(settingUIDIndex, settingAttribute);
    }
#endif // UNITY_EDITOR

    public override void Awake()
    {
        SetupDropdowns();
    }

    private void SetupDropdowns()
    {
        // add in the quality options
        QualityOptions = new List<string>(QualitySettings.names);
        QualitySelector.ClearOptions();
        QualitySelector.AddOptions(QualityOptions);

        // build up the list of resolutions
        foreach(Resolution candidateResolution in Screen.resolutions)
        {
            Resolutions.Add(candidateResolution);
            ResolutionOptions.Add(string.Format("{0} x {1} @ {2} Hz", candidateResolution.width, candidateResolution.height, candidateResolution.refreshRate));
        }
        ResolutionSelector.ClearOptions();
        ResolutionSelector.AddOptions(ResolutionOptions);
    }

    public override void PopulateInitialValue()
    {
        
    }

    float ScoreResolution(Resolution toScore, Resolution current)
    {
        float score = 0;

        // score is based on the relative difference between the proposed resolution and the current one
        score += Mathf.Abs((float)(toScore.width - current.width) / current.width);
        score += Mathf.Abs((float)(toScore.height - current.height) / current.height);
        score += Mathf.Abs((float)(toScore.refreshRate - current.refreshRate) / current.refreshRate);

        return score;
    }

    public override void SynchroniseUIWithSettings()
    {
        GraphicsQualityAndResolution currentSettings = SettingsBinder.GetGraphicsQualityAndResolution(UniqueID, SettingsManager.Settings);
        
        InitialQualityLevel = currentSettings.QualityLevel;
        QualitySelector.SetValueWithoutNotify(InitialQualityLevel);

        // find a matching resolution
        int bestResolution = -1;
        float bestScore = float.MaxValue;
        for(int index = 0; index < Resolutions.Count; ++index)
        {
            float score = ScoreResolution(Resolutions[index], Screen.currentResolution);

            if (score < bestScore)
            {
                bestScore = score;
                bestResolution = index;
            }
        }
        InitialResolution = bestResolution;

        ResolutionSelector.SetValueWithoutNotify(InitialResolution);

        InitialIsFullscreen = currentSettings.FullScreen;

        InitialGraphicsQualityAndResolution = new GraphicsQualityAndResolution(Resolutions[InitialResolution], InitialQualityLevel, InitialIsFullscreen);
        CurrentGraphicsQualityAndResolution = new GraphicsQualityAndResolution(Resolutions[InitialResolution], InitialQualityLevel, InitialIsFullscreen);
    }

    public override void ResetToOriginalValue()
    {
        QualitySelector.SetValueWithoutNotify(InitialQualityLevel);
        ResolutionSelector.SetValueWithoutNotify(InitialResolution);

        SettingsBinder.SetGraphicsQualityAndResolution(UniqueID, InitialGraphicsQualityAndResolution, SettingsManager.Settings);
    }

    public void OnResolutionSelected(int newValue)
    {
        CurrentGraphicsQualityAndResolution.SetResolution(Resolutions[newValue]);

        SettingsBinder.SetGraphicsQualityAndResolution(UniqueID, CurrentGraphicsQualityAndResolution, SettingsManager.Settings);
    }

    public void OnQualitySelected(int newValue)
    {
        CurrentGraphicsQualityAndResolution.SetQualityLevel(newValue);

        SettingsBinder.SetGraphicsQualityAndResolution(UniqueID, CurrentGraphicsQualityAndResolution, SettingsManager.Settings);
    }
}
