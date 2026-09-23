using System.Collections;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

/// <summary>
/// Lobs a bomb in an arc at a target point, shows a landing indicator, and damages
/// the player if they're inside the blast radius when it lands.
/// Same behaviour as BomberAI's bomb, packaged so other enemies can reuse it.
/// </summary>
public class BombLobber : MonoBehaviour
{
    [Header("Projectile Visual")]
    public Sprite projectileSprite;
    public float projectileScale = 1f;
    [SerializeField] private float projectileZ = -0.2f;
    [SerializeField] private float spinDegreesPerSecond = 720f;
    [SerializeField] private float endScaleMultiplier = 0.25f;

    [Header("Arc")]
    [SerializeField] private float flightTime = 0.9f;
    [SerializeField] private float arcHeightMultiplier = 0.35f;
    [SerializeField] private float minArcHeight = 0.4f;
    [SerializeField] private float maxArcHeight = 2.5f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private List<Constants.StatusEffects> statusEffects = new() { Constants.StatusEffects.Poisoned };
    [SerializeField] private Color particleColor = Color.green;
    [SerializeField] private int particleCount = 18;
    [SerializeField] private Constants.Sounds launchSound = Constants.Sounds.SpiderShoot;

    private readonly List<GameObject> _activeObjects = new();
    private Material _fillMaterial;
    private Material _borderMaterial;

    public void Lob(Vector2 target)
    {
        var indicator = CreateLandingIndicator(target, explosionRadius);
        _activeObjects.Add(indicator);

        var bomb = CreateVisualProjectile(transform.position);
        if (bomb)
        {
            _activeObjects.Add(bomb);
            StartCoroutine(ArcMove(bomb.transform, transform.position, target, flightTime));
        }

        StartCoroutine(DelayedExplosion(target, flightTime, indicator));
        AudioManager.Singleton.PlaySoundCached(launchSound);
    }

    private IEnumerator DelayedExplosion(Vector2 worldPos, float delay, GameObject indicator)
    {
        yield return new WaitForSeconds(delay);

        if (indicator)
        {
            _activeObjects.Remove(indicator);
            Destroy(indicator);
        }

        var hits = Physics2D.OverlapCircleAll(worldPos, explosionRadius);
        foreach (var col in hits)
        {
            if (!col) continue;

            var collidable = col.GetComponent<Collidable>();
            if (!collidable || !collidable.collisionGroups.Contains(Constants.CollisionGroups.Player)) continue;

            col.SendMessage(
                Constants.NPCMessages.ReceiveDamage,
                new DamageModel(damage, new List<Constants.StatusEffects>(statusEffects)),
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    private IEnumerator ArcMove(Transform tr, Vector2 start, Vector2 end, float time)
    {
        float elapsed = 0f;
        float height = Mathf.Clamp(Vector2.Distance(start, end) * arcHeightMultiplier, minArcHeight, maxArcHeight);

        Vector3 startScale = tr.localScale;
        Vector3 endScale = startScale * Mathf.Clamp(endScaleMultiplier, 0.01f, 1f);

        while (elapsed < time)
        {
            if (!tr) yield break;

            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / time);

            var pos = Vector2.Lerp(start, end, u);
            float arc = 4f * height * u * (1f - u);

            tr.position = new Vector3(pos.x, pos.y + arc, projectileZ);
            tr.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
            tr.localScale = Vector3.Lerp(startScale, endScale, u);

            yield return null;
        }

        if (!tr) yield break;

        tr.position = new Vector3(end.x, end.y, projectileZ);
        ParticleManager.Singleton.SpawnParticles(tr, particleColor, particleCount);

        _activeObjects.Remove(tr.gameObject);
        Destroy(tr.gameObject);
    }

    private GameObject CreateVisualProjectile(Vector2 startPos)
    {
        if (!projectileSprite) return null;

        var go = new GameObject("BombProjectile");
        go.transform.position = new Vector3(startPos.x, startPos.y, projectileZ);
        go.transform.localScale = Vector3.one * Mathf.Max(0.0001f, projectileScale);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = projectileSprite;
        sr.sortingLayerName = "Projectiles";
        sr.sortingOrder = 9999;

        return go;
    }

    private GameObject CreateLandingIndicator(Vector2 center, float radius)
    {
        // Materials are created once per lobber instead of once per bomb
        if (!_fillMaterial)
        {
            var shader = Shader.Find("Sprites/Default");
            _fillMaterial = new Material(shader) { color = new Color(1f, 0f, 0f, 0.25f) };
            _borderMaterial = new Material(shader) { color = new Color(1f, 0f, 0f, 1f) };
        }

        var root = new GameObject("BombLandingIndicator");
        root.transform.position = new Vector3(center.x, center.y, -0.1f);

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(root.transform, false);
        fillGo.AddComponent<MeshFilter>().mesh = CreateCircleMesh(radius, 64);
        var meshRenderer = fillGo.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = _fillMaterial;
        meshRenderer.sortingLayerName = "Actor";
        meshRenderer.sortingOrder = 1999;

        var borderGo = new GameObject("Border");
        borderGo.transform.SetParent(root.transform, false);
        var lr = borderGo.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.12f;
        lr.endWidth = 0.12f;
        lr.sharedMaterial = _borderMaterial;
        lr.sortingLayerName = "Actor";
        lr.sortingOrder = 2000;

        const int segments = 96;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float ang = i / (float)segments * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius, 0f));
        }

        return root;
    }

    private static Mesh CreateCircleMesh(float radius, int segments)
    {
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2 > segments ? 1 : i + 2;
        }

        var mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnDestroy()
    {
        // Bombs in flight vanish with the thrower, same as BomberAI
        foreach (var obj in _activeObjects)
        {
            if (obj) Destroy(obj);
        }
        _activeObjects.Clear();

        if (_fillMaterial) Destroy(_fillMaterial);
        if (_borderMaterial) Destroy(_borderMaterial);
    }
}
