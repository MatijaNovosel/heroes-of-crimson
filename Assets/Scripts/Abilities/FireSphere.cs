using System.Collections;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class FireSphere : MonoBehaviour
{
    [SerializeField] private GameObject impactParticlePrefab;
    
    private void SpawnParticles()
    {
        var position = transform.position;
        position.y -= 0.4f;
        
        for (int i = 0; i < 50; i++)
        {
            var p = Instantiate(
                impactParticlePrefab,
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f))
            );
      
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.6f);
            p.GetComponent<ImpactParticle>().Init(Color.orangeRed);
        }
    }

    private IEnumerator SpawnProjectilesCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        SpawnParticles();
    }

    public void Init()
    {
        AudioManager.Singleton.PlaySoundCached(Constants.Sounds.FireSphere);
        SpawnParticles();
        StartCoroutine(SpawnProjectilesCoroutine(4.9f));
        Destroy(gameObject, 5f);
    }
}
