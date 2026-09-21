using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseMenuOpen;
    public static PauseMenu Singleton;

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

        PauseMenuOpen = !PauseMenuOpen;
        Time.timeScale = PauseMenuOpen ? 0f : 1f;
        transform.localPosition = new Vector3(PauseMenuOpen ? 0 : 9999, PauseMenuOpen ? 0 : 9999, 0);
    }

    public void ResumeGame()
    {
        PauseMenuOpen = false;
        Time.timeScale = 1f;
        transform.localPosition = new Vector3(9999, 9999, 0);
    }

    public void ExitGame()
    {
        Application.Quit();
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
    }

    void Update()
    {
        HandleUIKeys();
    }
}
