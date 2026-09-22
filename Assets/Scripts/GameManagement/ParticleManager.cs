using System.Collections.Generic;
using System.Linq;
using GameManagement;
using JetBrains.Annotations;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private int prewarmCount = 150;

    public static ParticleManager Singleton;

    private readonly Stack<ImpactParticle> _pool = new();

    private void Awake()
    {
        Singleton = this;

        for (int i = 0; i < prewarmCount; i++)
        {
            _pool.Push(CreateParticle());
        }
    }

    private ImpactParticle CreateParticle()
    {
        var obj = Instantiate(impactParticlePrefab);
        obj.SetActive(false);
        return obj.GetComponent<ImpactParticle>();
    }

    public void Release(ImpactParticle particle)
    {
        particle.gameObject.SetActive(false);
        _pool.Push(particle);
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
            var p = _pool.Count > 0 ? _pool.Pop() : CreateParticle();

            p.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f))
            );
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.6f);
            p.gameObject.SetActive(true);
            p.Init(color, sprite);
        }
    }
}