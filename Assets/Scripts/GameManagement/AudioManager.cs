using GameManagement;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton;
    public Player player;
    
    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }
    
    private void Update()
    {
        if (Utils.IsPlayerDead()) return;
        transform.position = player.transform.position;
    }

    public void PlaySound(AudioClip sound)
    {
        AudioSource.PlayClipAtPoint(sound, transform.position, 1.5f);
    }
    
    public void PlaySoundCached(Constants.Sounds value)
    {
        AudioSource.PlayClipAtPoint(
            ResourceCacher.Singleton.Sounds[value],
            transform.position, 
            1.5f
        );
    }
}
