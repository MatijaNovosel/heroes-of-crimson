using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseMenuOpen;
    
    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuOpen = !PauseMenuOpen;
            Time.timeScale = PauseMenuOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(PauseMenuOpen ? 0 : 9999, PauseMenuOpen ? 0 : 9999, 0);
        }
    }
    
    public void ResumeGame()
    {
        PauseMenuOpen = false;
        Time.timeScale = 1f;
        this.transform.localPosition = new Vector3(9999, 9999, 0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
        PauseMenuOpen = false;
        Time.timeScale = PauseMenuOpen ? 0f : 1f;
    }

    void Update()
    {
        HandleUIKeys();
    }
}
