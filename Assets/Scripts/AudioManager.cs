using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton;
    public Player player;
    
    private void Awake()
    {
        Singleton = this;
    }
    
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update()
    {
        transform.position = player.transform.position;
    }

    public void PlaySound(AudioClip sound)
    {
        AudioSource.PlayClipAtPoint(sound, transform.position, 1.5f);
    }
}
