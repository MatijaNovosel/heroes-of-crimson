using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
    }

    public void NewGame()
    {
        SceneManager.LoadScene((int)Constants.Screens.NewGame);
    }
    
    public void Continue()
    {
        SceneManager.LoadScene((int)Constants.Screens.Continue);
    }
}
