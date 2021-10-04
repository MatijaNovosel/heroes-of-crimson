using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  private float moveSpeed = 1f;
  public Animator animator;
  private AnimatorOverrideController animatorOverrideController;

  public AnimationClip playerIdleUp;
  public AnimationClip playerIdleDown;
  public AnimationClip playerIdleLeftOrRight;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  public GameObject Projectile;
  private float lastShown;
  private readonly float delay = 0.2f; // 200 ms

  bool CanFire()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - lastShown > delay;
  }

  void Fire()
  {
    Vector3 shootDirection = (Utils.GetMousePosition() - gameObject.transform.position).normalized;

    float angle = Utils.GetAngleFromShootDirection(shootDirection);

    /*
      |----------- 90
      |            |
      |            |
    180, -180 ----|---- 0
          |       |
          |       |
         |----- -90
    */

    if (angle > 45 && angle < 135)
    {
      // Up
      animatorOverrideController["playerIdle"] = playerIdleUp;
    }
    else if (angle < 45 && angle > -45)
    {
      // Right
      animatorOverrideController["playerIdle"] = playerIdleLeftOrRight;
      transform.localScale = Vector3.one;
    }
    else if ((angle > 135 && angle < 180) || (angle > -180 && angle < -135))
    {
      // Left
      animatorOverrideController["playerIdle"] = playerIdleLeftOrRight;
      transform.localScale = new Vector3(-1, 1, 1);
    }
    else if (angle < -45 && angle > -135)
    {
      // Down
      animatorOverrideController["playerIdle"] = playerIdleDown;
    }

    GameObject proj = Instantiate(
      Projectile,
      new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0),
      Quaternion.identity
    );

    proj.GetComponent<Projectile>().Setup(shootDirection);
    lastShown = Time.time;
  }

  private void HandleMoving()
  {
    float x = Input.GetAxisRaw("Horizontal");
    float y = Input.GetAxisRaw("Vertical");

    // Reset moveDelta
    moveDelta = new Vector3(x, y, 0);

    animator.SetFloat("Horizontal", x);
    animator.SetFloat("Vertical", y);
    animator.SetFloat("Speed", moveDelta.sqrMagnitude);

    if (moveDelta.x > 0)
    {
      transform.localScale = Vector3.one;
    }
    else if (moveDelta.x < 0)
    {
      transform.localScale = new Vector3(-1, 1, 1);
    }

    if (Input.GetKey(KeyCode.W))
    {
      animatorOverrideController["playerIdle"] = playerIdleUp;
    }

    if (Input.GetKey(KeyCode.S))
    {
      animatorOverrideController["playerIdle"] = playerIdleDown;
    }

    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
    {
      animatorOverrideController["playerIdle"] = playerIdleLeftOrRight;
    }

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(0, moveDelta.y),
      Mathf.Abs(moveDelta.y * Utils.CalculatePlayerMovementSpeed(moveSpeed)),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(0, moveDelta.y * Utils.CalculatePlayerMovementSpeed(moveSpeed), 0);
    }

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(moveDelta.x, 0),
      Mathf.Abs(moveDelta.x * Utils.CalculatePlayerMovementSpeed(moveSpeed)),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(moveDelta.x * Utils.CalculatePlayerMovementSpeed(moveSpeed), 0, 0);
    }
  }

  private void HandleShooting()
  {
    if (Input.GetMouseButton(0))
    {
      if (CanFire())
      {
        Fire();
      }
    }
  }

  private void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = animatorOverrideController;
  }

  private void FixedUpdate()
  {
    HandleShooting();
    HandleMoving();
  }
}
