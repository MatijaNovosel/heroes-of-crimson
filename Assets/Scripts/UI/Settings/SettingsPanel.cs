using System;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public enum CloseAction
    {
        Hide,
        LoadMainMenu
    }

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private CloseAction closeAction = CloseAction.Hide;

    public event Action Closed;

    public bool IsOpen => gameObject.activeInHierarchy;

    private void OnEnable()
    {
        RefreshControls();
    }

    public void Open()
    {
        FillParent();
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        RefreshControls();
    }

    private void FillParent()
    {
        if (transform is not RectTransform rect) return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    public void Save()
    {
        var settings = GameSettings.Current.Clone();
        if (fullscreenToggle) settings.fullscreen = fullscreenToggle.isOn;

        GameSettings.Save(settings);
        Close();
    }

    public void Back()
    {
        Close();
    }

    private void RefreshControls()
    {
        var settings = GameSettings.Current;
        if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(settings.fullscreen);
    }

    private void Close()
    {
        Closed?.Invoke();

        if (closeAction == CloseAction.LoadMainMenu)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene((int)Constants.Screens.MainMenu);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}