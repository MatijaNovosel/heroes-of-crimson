using HeroesOfCrimson.Utils;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Player _player;
    private Animator _animator;
    private Vector3 _moveDelta;
    private BoxCollider2D _boxCollider;
    
    private static readonly int AnimationIdleStateKey = Animator.StringToHash("IdleState");
    
    private void HandleMoving()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        var input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input = input.normalized;

        _moveDelta = new Vector3(input.x, input.y, 0);

        _player.animator.SetFloat("Horizontal", input.x);
        _player.animator.SetFloat("Vertical", input.y);
        _player.animator.SetFloat("Speed", input.sqrMagnitude);
        
        if (!_player.isShooting)
        {
            if (Input.GetKey(KeyCode.W))
            {
                _animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Up);
            }

            if (Input.GetKey(KeyCode.S))
            {
                _animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Down);
            }

            if (Input.GetKey(KeyCode.A))
            {
                _animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Horizonal);
                transform.localScale = new Vector3(-1, 1, 1);
            }

            if (Input.GetKey(KeyCode.D))
            {
                _animator.SetFloat(AnimationIdleStateKey, (int)Constants.AnimationIdleState.Horizonal);
                transform.localScale = Vector3.one;
            }
        }

        if (_player.HasStatusEffect(Constants.StatusEffects.Paralyzed)) return;

        bool slowed = _player.HasStatusEffect(Constants.StatusEffects.Slowed);
        bool energized = _player.HasStatusEffect(Constants.StatusEffects.Energized);

        float unitsPerSecond = Utils.CalculatePlayerMovementSpeed(
            _player.actualSwf,
            slowed,
            energized
        );

        float step = unitsPerSecond * Time.fixedDeltaTime;

        LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

        var moveY = new Vector2(0f, input.y);
        if (moveY.y != 0f && !Physics2D.BoxCast(
            transform.position,
            _boxCollider.size,
            0,
            moveY,
            step,
            mask 
        ))
        {
            transform.Translate(0, moveY.y * step, 0);
        }

        var moveX = new Vector2(input.x, 0f);
        if (moveX.x != 0f && !Physics2D.BoxCast(
            transform.position,
            _boxCollider.size,
            0,
            moveX,
            step,
            mask
        ))
        {
            transform.Translate(moveX.x * step, 0, 0);
        }
    }
    
    void Start()
    {
        _player = GetComponent<Player>();
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        HandleMoving();
    }
}
