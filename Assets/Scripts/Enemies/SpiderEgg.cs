using UnityEngine;

public class SpiderEgg : BaseNPCBehaviour
{
    [Header("Spider Egg")]
    [SerializeField] private GameObject spiderPrefab;
    [SerializeField] private int spiderCount = 1;
    [SerializeField] private float spawnRadius = 1.5f;

    private bool hasSpawnedSpiders;

    public override void Die()
    {
        SpawnSpiders();
        base.Die();
    }

    private void SpawnSpiders()
    {
        if (hasSpawnedSpiders) return;

        hasSpawnedSpiders = true;

        if (spiderPrefab == null)
        {
            Debug.LogWarning($"{name}: No spider prefab assigned.");
            return;
        }

        for (int i = 0; i < spiderCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

            Vector3 spawnPosition = transform.position + new Vector3(
                randomOffset.x,
                randomOffset.y,
                0f
            );

            Instantiate(
                spiderPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }
}