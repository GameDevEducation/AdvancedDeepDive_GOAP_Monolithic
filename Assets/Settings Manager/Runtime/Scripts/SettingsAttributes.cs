using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = false)]
public abstract class ConfigurableSetting : System.Attribute
{
    public string Category;
    public string DisplayName;
    public string HelpText;
    public string ApplyMethod;
    public bool ForceRefreshOnChange;

    public ConfigurableSetting(string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false)
    {
        Category                = _category;
        DisplayName             = _displayName;
        HelpText                = _helpText;
        ApplyMethod             = _applyMethod;
        ForceRefreshOnChange    = _forceRefreshOnChange;
    }
}

public class IntegerSetting : ConfigurableSetting
{
    public int MinValue;
    public int MaxValue;
    public int DefaultValue;

    public IntegerSetting(int _minValue, int _maxValue, int _defaultValue, string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false) :
        base(_category, _displayName, _helpText, _applyMethod, _forceRefreshOnChange)
    {
        MinValue        = _minValue;
        MaxValue        = _maxValue;
        DefaultValue    = _defaultValue;
    }
}

public class StringSetting : ConfigurableSetting
{
    public string DefaultValue;

    public StringSetting(string _defaultValue, string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false) :
        base(_category, _displayName, _helpText, _applyMethod, _forceRefreshOnChange)
    {
        DefaultValue = _defaultValue;
    }
}

public class FloatSetting : ConfigurableSetting
{
    public float MinValue;
    public float MaxValue;
    public float DefaultValue;

    public FloatSetting(float _minValue, float _maxValue, float _defaultValue, string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false) :
        base(_category, _displayName, _helpText, _applyMethod, _forceRefreshOnChange)
    {
        MinValue        = _minValue;
        MaxValue        = _maxValue;
        DefaultValue    = _defaultValue;
    }
}

public class BooleanSetting : ConfigurableSetting
{
    public bool DefaultValue;

    public BooleanSetting(bool _defaultValue, string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false) :
        base(_category, _displayName, _helpText, _applyMethod, _forceRefreshOnChange)
    {
        DefaultValue = _defaultValue;   
    }
}

public class GraphicsQualityAndResolutionSetting : ConfigurableSetting
{
    public GraphicsQualityAndResolutionSetting(string _category, string _displayName, string _helpText, string _applyMethod = null, bool _forceRefreshOnChange = false) :
        base(_category, _displayName, _helpText, _applyMethod, _forceRefreshOnChange)
    {
        
    }    
}
