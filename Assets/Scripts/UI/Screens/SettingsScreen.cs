using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Legacy settings screen script. Settings now live in GameSettings, and the UI in the
/// shared SettingsPanel prefab. This stays so the Settings scene keeps working
/// until the panel is set up (Tools > Heroes of Crimson > Settings).
/// </summary>
public class SettingsScreen : MonoBehaviour
{
    [SerializeField]
    private Toggle fullscreenToggle;

    private void Start()
    {
        if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Current.fullscreen);
    }

    public void Back()
    {
        SceneManager.LoadScene((int)Constants.Screens.MainMenu);
    }

    public void Save()
    {
        var settings = GameSettings.Current.Clone();
        if (fullscreenToggle) settings.fullscreen = fullscreenToggle.isOn;
        GameSettings.Save(settings);
        SceneManager.LoadScene((int)Constants.Screens.MainMenu);
    }
}
