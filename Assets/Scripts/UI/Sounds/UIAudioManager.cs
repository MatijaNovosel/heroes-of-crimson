using GameManagement;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Singleton;
    public AudioClip uiClickSound;
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayUIClick()
    {
        GetComponent<AudioSource>().PlayOneShot(uiClickSound);
    }
}