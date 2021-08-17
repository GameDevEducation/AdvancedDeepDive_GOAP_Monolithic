public enum SettingUID
{
    Unknown,
    Setting_1_Graphics_QualityAndResolution,
    Setting_2_Camera_Invert_YAxis,
    Setting_3_Camera_Sensitivity_X,
    Setting_4_Camera_Sensitivity_Y,
    Setting_5_Haptics_Enabled,
    NumSettings
}
static public class SettingsBinder
{
    public static string GetString(SettingUID uniqueID, SettingsData settings)
    {
        switch(uniqueID)
        {
            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static void SetString(SettingUID uniqueID, string newValue, SettingsData settings)
    {
        switch(uniqueID)
        {
            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static bool GetBoolean(SettingUID uniqueID, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_2_Camera_Invert_YAxis:
                return settings.Camera.Invert_YAxis;

            case SettingUID.Setting_5_Haptics_Enabled:
                return settings.Haptics.Enabled;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static void SetBoolean(SettingUID uniqueID, bool newValue, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_2_Camera_Invert_YAxis:
                settings.Camera.Invert_YAxis = newValue;
                return;

            case SettingUID.Setting_5_Haptics_Enabled:
                settings.Haptics.Enabled = newValue;
                settings.Haptics.ApplySettings();
                return;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static int GetInteger(SettingUID uniqueID, SettingsData settings)
    {
        switch(uniqueID)
        {
            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static void SetInteger(SettingUID uniqueID, int newValue, SettingsData settings)
    {
        switch(uniqueID)
        {
            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static float GetFloat(SettingUID uniqueID, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_3_Camera_Sensitivity_X:
                return settings.Camera.Sensitivity_X;

            case SettingUID.Setting_4_Camera_Sensitivity_Y:
                return settings.Camera.Sensitivity_Y;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static void SetFloat(SettingUID uniqueID, float newValue, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_3_Camera_Sensitivity_X:
                settings.Camera.Sensitivity_X = newValue;
                return;

            case SettingUID.Setting_4_Camera_Sensitivity_Y:
                settings.Camera.Sensitivity_Y = newValue;
                return;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static GraphicsQualityAndResolution GetGraphicsQualityAndResolution(SettingUID uniqueID, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_1_Graphics_QualityAndResolution:
                return settings.Graphics.QualityAndResolution;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
    public static void SetGraphicsQualityAndResolution(SettingUID uniqueID, GraphicsQualityAndResolution newValue, SettingsData settings)
    {
        switch(uniqueID)
        {
            case SettingUID.Setting_1_Graphics_QualityAndResolution:
                settings.Graphics.QualityAndResolution.UpdateFrom(newValue);
                settings.Graphics.ApplySettings();
                return;

            default:
                break;
        }
        throw new System.ArgumentException("Unable to find setting " + uniqueID + ". You may need to regenerate the Settings UI.");
    }
}
