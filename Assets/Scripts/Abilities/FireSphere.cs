using System.Collections;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class FireSphere : MonoBehaviour
{
    private IEnumerator SpawnProjectilesCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        ParticleManager.Singleton.SpawnParticles(transform, Color.orange, 50);
    }

    public void Init()
    {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.FireSphere);
        ParticleManager.Singleton.SpawnParticles(transform, Color.orange, 50);
        StartCoroutine(SpawnProjectilesCoroutine(4.9f));
        Destroy(gameObject, 5f);
    }
}
