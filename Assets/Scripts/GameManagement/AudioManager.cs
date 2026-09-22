using GameManagement;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton;
    public Player player;
    private AudioSource _sfxSource;

    private void Awake()
    {
        Singleton = this;

        var sfxObject = new GameObject("SFX Source");
        sfxObject.transform.SetParent(transform, false);

        _sfxSource = sfxObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 1f;
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
        if (!sound) return;
        _sfxSource.PlayOneShot(sound);
    }
    
    public void PlaySoundCached(Constants.Sounds value)
    {
        PlaySound(ResourceCacher.Singleton.Sounds[value]);
    }
}