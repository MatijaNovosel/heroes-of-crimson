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
        _moveDelta = new Vector3(x, y, 0);

        _player.animator.SetFloat("Horizontal", x);
        _player.animator.SetFloat("Vertical", y);
        _player.animator.SetFloat("Speed", _moveDelta.sqrMagnitude);

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

        var speed = Utils.CalculatePlayerMovementSpeed(_player.actualSwf);
        LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");
        var moveY = new Vector2(0, _moveDelta.y);

        if (_player.HasStatusEffect(Constants.StatusEffects.Paralyzed)) return;
    
        if (!Physics2D.BoxCast(
            transform.position,
            _boxCollider.size, 
            0, 
            moveY, 
            Mathf.Abs(moveY.y * speed), mask)
           )
        {
            transform.Translate(0, moveY.y * speed, 0);
        }

        var moveX = new Vector2(_moveDelta.x, 0);
    
        if (!Physics2D.BoxCast(
            transform.position,
            _boxCollider.size, 
            0, 
            moveX, 
            Mathf.Abs(moveX.x * speed), mask)
           )
        {
            transform.Translate(moveX.x * speed, 0, 0);
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
