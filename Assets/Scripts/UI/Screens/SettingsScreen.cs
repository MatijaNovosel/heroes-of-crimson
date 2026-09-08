using System.IO;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField]
    private Toggle fullscreenToggle;

    private SettingsData settings;

    private string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");

    private void Start()
    {
        LoadSettings();
        fullscreenToggle.isOn = settings.fullscreen;
    }

    public void Back()
    {
        SceneManager.LoadScene((int)Constants.Screens.MainMenu);
    }

    public void Save()
    {
        settings.fullscreen = fullscreenToggle.isOn;
        ApplySettings();
        SaveSettings();
        SceneManager.LoadScene((int)Constants.Screens.MainMenu);
    }

    private void ApplySettings()
    {
        if (settings.fullscreen) {
            Screen.SetResolution(
                Display.main.systemWidth,
                Display.main.systemHeight,
                FullScreenMode.FullScreenWindow
            );
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(SettingsPath, json);
        Debug.Log($"Settings saved to: {SettingsPath}");
    }

    private void LoadSettings()
    {
        if (File.Exists(SettingsPath))
        {
            string json = File.ReadAllText(SettingsPath);
            settings = JsonUtility.FromJson<SettingsData>(json);
        } else {
            settings = new SettingsData
            {
                fullscreen = Screen.fullScreen
            };
            SaveSettings();
        }
        ApplySettings();
    }
}

[System.Serializable]
public class SettingsData
{
    public bool fullscreen = true;
}