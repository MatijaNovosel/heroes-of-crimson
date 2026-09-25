using System;
using System.IO;
using UnityEngine;

public static class GameSettings
{
    private static SettingsData _current;
    public static event Action Changed;

    private static string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");

    public static SettingsData Current
    {
        get
        {
            if (_current == null) Load();
            return _current;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _current = null;
        Load();
        Apply();
    }

    public static void Save(SettingsData settings)
    {
        _current = settings.Clone();
        Apply();
        WriteToDisk();
        Changed?.Invoke();
    }

    private static void Apply()
    {
        if (_current.fullscreen)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                _current = JsonUtility.FromJson<SettingsData>(File.ReadAllText(SettingsPath));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't read settings, using defaults: {e.Message}");
        }

        if (_current == null)
        {
            _current = new SettingsData { fullscreen = Screen.fullScreen };
            WriteToDisk();
        }
    }

    private static void WriteToDisk()
    {
        try
        {
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(_current, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't save settings: {e.Message}");
        }
    }
}

[Serializable]
public class SettingsData
{
    public bool fullscreen = true;

    public SettingsData Clone() => (SettingsData)MemberwiseClone();
}
