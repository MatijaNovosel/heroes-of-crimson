using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathOverlay : MonoBehaviour
{
    public static DeathOverlay Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    public void Show()
    {
        Time.timeScale = 0f;
        transform.localPosition = Vector3.zero;
    }
    
    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        transform.localPosition = Vector3.positiveInfinity;
        SceneManager.LoadScene((int)Constants.Screens.MainMenu);
    }
}
