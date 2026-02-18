using System.Linq;
using GameManagement;
using JetBrains.Annotations;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private GameObject impactParticlePrefab;
    public static ParticleManager Singleton;

    private void Awake()
    {
        Singleton = this;
    }

    public void SpawnParticles(
        Transform transformParam,
        Color color,
        int amount,
        [CanBeNull] string spritePath = null
    )
    {
        var position = transformParam.position;
        position.y -= 0.4f;

        Sprite sprite = null;

        if (spritePath != null)
        {
            sprite = ResourceCacher.Singleton.ProjectileSprites.FirstOrDefault(x => x.name == spritePath);
        }
        
        for (int i = 0; i < amount; i++)
        {
            var p = Instantiate(
                impactParticlePrefab,
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f))
            );

            if (sprite != null) p.GetComponent<SpriteRenderer>().sprite = sprite;
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.6f);
            p.GetComponent<ImpactParticle>().Init(color);
        }
    }
}
