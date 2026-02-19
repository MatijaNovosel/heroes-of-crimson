using UnityEngine;

public class ImpactParticle : MonoBehaviour
{
    private Vector2 _velocity;
    private float _life;
    private SpriteRenderer _sr;

    public void Init(Color color)
    {
        _sr = GetComponent<SpriteRenderer>();
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
        if (_life <= 0f) Destroy(gameObject);
    }
}