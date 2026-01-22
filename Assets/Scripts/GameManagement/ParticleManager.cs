using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private GameObject impactParticlePrefab;
    public static ParticleManager Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    public void SpawnParticles(Transform transformParam, Color color, int amount)
    {
        var position = transformParam.position;
        position.y -= 0.4f;
        
        for (int i = 0; i < amount; i++)
        {
            var p = Instantiate(
                impactParticlePrefab,
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f))
            );
      
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.6f);
            p.GetComponent<ImpactParticle>().Init(color);
        }
    }
}
