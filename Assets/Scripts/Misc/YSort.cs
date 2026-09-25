using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float ySortOffset = 0f;
    [SerializeField] private bool fadeWhenPlayerBehind = true;
    [SerializeField] private float fadedAlpha = 0.45f;
    [SerializeField] private float fadeSpeed = 6f;

    private bool _canFade;
    private float _baseAlpha;
    private float _fade = 1f; // 1 = normal, fadedAlpha = see-through

    private static SpriteRenderer _playerRenderer;
    private static YSort _playerYSort;
    private static Transform _cachedPlayer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _baseAlpha = spriteRenderer.color.a;
        _canFade = fadeWhenPlayerBehind && !GetComponent<BaseNPCBehaviour>() && !GetComponent<LootBag>();
    }

    void LateUpdate()
    {
        float sortY = SortY;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(sortY * -100);

        if (_canFade) UpdateFade(sortY);
    }

    private float SortY => transform.position.y + ySortOffset;

    private void UpdateFade(float sortY)
    {
        float target = IsPlayerBehind(sortY) ? fadedAlpha : 1f;
        if (Mathf.Approximately(_fade, target)) return;
        _fade = Mathf.MoveTowards(_fade, target, fadeSpeed * Time.deltaTime);
        var color = spriteRenderer.color;
        color.a = _baseAlpha * _fade;
        spriteRenderer.color = color;
    }
    
    private bool IsPlayerBehind(float sortY)
    {
        if (!CachePlayer()) return false;

        float playerSortY = _playerYSort ? _playerYSort.SortY : _cachedPlayer.position.y;
        if (playerSortY <= sortY) return false;

        var mine = spriteRenderer.bounds;
        var theirs = _playerRenderer
            ? _playerRenderer.bounds
            : new Bounds(_cachedPlayer.position, new Vector3(0.5f, 0.8f, 0f));

        return mine.min.x < theirs.max.x && theirs.min.x < mine.max.x
            && mine.min.y < theirs.max.y && theirs.min.y < mine.max.y;
    }

    private static bool CachePlayer()
    {
        var player = Player.Singleton;
        if (!player) return false;

        if (_cachedPlayer != player.transform)
        {
            _cachedPlayer = player.transform;
            _playerRenderer = player.GetComponent<SpriteRenderer>();
            _playerYSort = player.GetComponent<YSort>();
        }

        return true;
    }
}