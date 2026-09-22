using UnityEngine;

public class ImpactParticle : MonoBehaviour
{
    private Vector2 _velocity;
    private float _life;
    private SpriteRenderer _sr;
    private Sprite _defaultSprite;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _defaultSprite = _sr.sprite;
    }

    public void Init(Color color, Sprite spriteOverride = null)
    {
        _sr.sprite = spriteOverride != null ? spriteOverride : _defaultSprite;
        _sr.color = color;

        _velocity = new Vector2(
            Random.Range(-2f, 2f),
            Random.Range(1f, 4f)
        );

        _life = Random.Range(0.4f, 0.8f);
    }

    void Update()
    {
        _velocity += Vector2.down * (10f * Time.deltaTime);
        transform.position += (Vector3)(_velocity * Time.deltaTime);

        Color c = _sr.color;
        c.a -= Time.deltaTime;
        _sr.color = c;

        _life -= Time.deltaTime;
        if (_life <= 0f) ParticleManager.Singleton.Release(this);
    }
}