using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseMenuOpen;
    public static PauseMenu Singleton;

    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private GameObject pauseContent;

    private bool IsBlockingMenuOpen()
    {
        return (TalentTree.Singleton != null && TalentTree.Singleton.TalentTreeOpen)
               || (DialogMenu.Singleton != null && DialogMenu.Singleton.DialogMenuOpen)
               || (ConsoleMenu.Singleton != null && ConsoleMenu.Singleton.ConsoleMenuOpen);
    }

    private void HandleUIKeys()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (IsBlockingMenuOpen()) return;

        if (settingsPanel && settingsPanel.IsOpen)
        {
            settingsPanel.Back();
            return;
        }

        PauseMenuOpen = !PauseMenuOpen;
        Time.timeScale = PauseMenuOpen ? 0f : 1f;
        transform.localPosition = new Vector3(PauseMenuOpen ? 0 : 9999, PauseMenuOpen ? 0 : 9999, 0);
    }

    public void ResumeGame()
    {
        CloseSettingsSilently();
        PauseMenuOpen = false;
        Time.timeScale = 1f;
        transform.localPosition = new Vector3(9999, 9999, 0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        if (!settingsPanel) return;
        if (pauseContent) pauseContent.SetActive(false);
        settingsPanel.Open();
    }

    private void OnSettingsClosed()
    {
        if (pauseContent) pauseContent.SetActive(true);
    }

    private void CloseSettingsSilently()
    {
        if (settingsPanel && settingsPanel.IsOpen) settingsPanel.gameObject.SetActive(false);
        if (pauseContent) pauseContent.SetActive(true);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
        PauseMenuOpen = false;
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        Singleton = this;

        if (!pauseContent)
        {
            var content = transform.Find("PauseMenuContent");
            if (content) pauseContent = content.gameObject;
        }

        if (settingsPanel)
        {
            settingsPanel.Closed += OnSettingsClosed;
            settingsPanel.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (settingsPanel) settingsPanel.Closed -= OnSettingsClosed;
    }

    void Update()
    {
        HandleUIKeys();
    }
}