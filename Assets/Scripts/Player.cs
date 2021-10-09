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
  public AnimationClip playerIdle;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  public GameObject Projectile;
  private float lastShown;
  private readonly float delay = 0.8f; // 200 ms
  private bool isShooting = false;

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
      animator.SetFloat("MouseDir", 4);
    }
    else if (angle < 45 && angle > -45)
    {
      // Right
      animatorOverrideController["playerIdle"] = playerIdle;
      transform.localScale = Vector3.one;
      animator.SetFloat("MouseDir", 2);
    }
    else if ((angle > 135 && angle < 180) || (angle > -180 && angle < -135))
    {
      // Left
      animatorOverrideController["playerIdle"] = playerIdle;
      transform.localScale = new Vector3(-1, 1, 1);
      animator.SetFloat("MouseDir", 2);
    }
    else if (angle < -45 && angle > -135)
    {
      // Down
      animatorOverrideController["playerIdle"] = playerIdleDown;
      animator.SetFloat("MouseDir", 0);
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

    if (!isShooting)
    {
      if (Input.GetKey(KeyCode.W))
      {
        animatorOverrideController["playerIdle"] = playerIdleUp;
      }

      if (Input.GetKey(KeyCode.S))
      {
        animatorOverrideController["playerIdle"] = playerIdleDown;
      }

      if (Input.GetKey(KeyCode.A))
      {
        animatorOverrideController["playerIdle"] = playerIdle;
        transform.localScale = new Vector3(-1, 1, 1);
      }

      if (Input.GetKey(KeyCode.D))
      {
        animatorOverrideController["playerIdle"] = playerIdle;
        transform.localScale = Vector3.one;
      }
    }

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(0, moveDelta.y),
      Mathf.Abs(moveDelta.y * Utils.CalculatePlayerMovementSpeed(moveSpeed)),
      LayerMask.GetMask("Actor", "Blocking", "NPC")
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
      LayerMask.GetMask("Actor", "Blocking", "NPC")
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
        isShooting = true;
        animator.SetBool("Shooting", isShooting);
        animator.SetTrigger("Shoot");
        Fire();
      }
    }
    if (Input.GetMouseButtonUp(0))
    {
      isShooting = false;
      animator.SetBool("Shooting", isShooting);
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
    HandleMoving();
  }

  private void Update()
  {
    HandleShooting();
  }
}
