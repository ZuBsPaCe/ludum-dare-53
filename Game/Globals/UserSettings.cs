using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class UserSettings : Node
{
    private static Dictionary<string, Variant> _settings;

    public static UserSettings Instance { get; set; }

    private static bool _initialized;

    public static float MusicVolume
    {
        get { return (float)_settings["MusicVolume"]; }
        set 
        { 
            _settings["MusicVolume"] = value;
            Save();
            EventHub.EmitMusicVolumeChanged(value);
        }
    }

    public static float SoundVolume
    {
        get { return (float)_settings["SoundVolume"]; }
        set
        {
            _settings["SoundVolume"] = value;
            Save();
            EventHub.EmitSoundVolumeChanged(value);
        }
    }

    public static bool Fullscreen
    {
        get { return (bool)_settings["Fullscreen"]; }
        set
        {
            _settings["Fullscreen"] = value;
            Save();
            EventHub.EmitFullscreenChanged(value);
        }
    }



    public static float SoundSettings { get; private set; }

    public static void Load()
    {
        if (_settings != null)
        {
            Debug.Fail("UserSettings.Load() should be called once in the beginning.");
            return;
        }

        _settings = new();

        Dictionary<string, Variant> userSettings = null;

        try
        {
            GD.Print($"Loading settings: {File.Exists("UserSettings.json")}");
            if (File.Exists("UserSettings.json"))
            {
                string content = File.ReadAllText("UserSettings.json");
                userSettings = (Dictionary<string, Variant>)Json.ParseString(content);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }

        MusicVolume = ReadFloat("MusicVolume", 0.8f, userSettings);
        SoundVolume = ReadFloat("SoundVolume", 0.8f, userSettings);
        Fullscreen = ReadBool("Fullscreen", true, userSettings);

        _initialized = true;

        Save();
    }

    private static void Save()
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            string content = Json.Stringify(_settings);
            File.WriteAllText("UserSettings.json", content);
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private static float ReadFloat(string name, float defaultValue, Dictionary<string, Variant> userSettings)
    {
        float finalValue = defaultValue;

        if (userSettings != null && userSettings.TryGetValue(name, out var data))
        {
            try
            {
                finalValue = data.AsSingle();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex.Message);
            }
        }

        return finalValue;
    }

    private static bool ReadBool(string name, bool defaultValue, Dictionary<string, Variant> userSettings)
    {
        bool finalValue = defaultValue;

        if (userSettings != null && userSettings.TryGetValue(name, out var data))
        {
            try
            {
                finalValue = data.AsBool();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex.Message);
            }
        }

        return finalValue;
    }
}
