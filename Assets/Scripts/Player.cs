using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;

public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  private readonly float moveSpeed = 100f;
  public Animator animator;
  private AnimatorOverrideController animatorOverrideController;
  private BaseNPCBehaviour baseNPCBehaviour;
  public HealthBar healthBar;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  public GameObject Projectile;
  private float lastShown;
  private readonly float delay = 0.3f; // 0.8f -> 200 ms
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

          ┌─────(90)────┐
          │      │      │
          │      │      │
       (+-180)───╀─────(0)
          │      │      │
          │      │      │
          └────(-90)────┘

    */
    switch (angle)
    {
      case float n when n > 45 && n < 135:
        // Up
        animator.SetFloat("MouseDir", (int)Constants.SHOOTING_MOUSE_DIRS.UP);
        animator.SetFloat("IdleState", 1);
        break;
      case float n when n < 45 && n > -45:
        // Right
        animator.SetFloat("IdleState", 0);
        transform.localScale = Vector3.one;
        animator.SetFloat("MouseDir", (int)Constants.SHOOTING_MOUSE_DIRS.HORIZONTAL);
        break;
      case float n when (n > 135 && n < 180) || (n > -180 && n < -135):
        // Left
        animator.SetFloat("IdleState", 0);
        transform.localScale = new Vector3(-1, 1, 1);
        animator.SetFloat("MouseDir", (int)Constants.SHOOTING_MOUSE_DIRS.HORIZONTAL);
        break;
      default:
        // Down
        animator.SetFloat("IdleState", 2);
        animator.SetFloat("MouseDir", (int)Constants.SHOOTING_MOUSE_DIRS.DOWN);
        break;
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
    moveDelta = new Vector3(x, y, 0);

    animator.SetFloat("Horizontal", x);
    animator.SetFloat("Vertical", y);
    animator.SetFloat("Speed", moveDelta.sqrMagnitude);

    if (!isShooting)
    {
      if (Input.GetKey(KeyCode.W))
      {
        animator.SetFloat("IdleState", 1);
      }

      if (Input.GetKey(KeyCode.S))
      {
        animator.SetFloat("IdleState", 2);
      }

      if (Input.GetKey(KeyCode.A))
      {
        animator.SetFloat("IdleState", 0);
        transform.localScale = new Vector3(-1, 1, 1);
      }

      if (Input.GetKey(KeyCode.D))
      {
        animator.SetFloat("IdleState", 0);
        transform.localScale = Vector3.one;
      }
    }

    float speed = Utils.CalculatePlayerMovementSpeed(moveSpeed);
    LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

    Vector2 moveY = new Vector2(0, moveDelta.y);
    if (!Physics2D.BoxCast(transform.position, boxCollider.size, 0, moveY, Mathf.Abs(moveY.y * speed), mask))
    {
      transform.Translate(0, moveY.y * speed, 0);
    }

    Vector2 moveX = new Vector2(moveDelta.x, 0);
    if (!Physics2D.BoxCast(transform.position, boxCollider.size, 0, moveX, Mathf.Abs(moveX.x * speed), mask))
    {
      transform.Translate(moveX.x * speed, 0, 0);
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
    baseNPCBehaviour = GetComponent<BaseNPCBehaviour>();
    boxCollider = GetComponent<BoxCollider2D>();
    animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = animatorOverrideController;
    baseNPCBehaviour.hp = 100;
  }

  private void FixedUpdate()
  {
    HandleMoving();
    healthBar.UpdateFillAmount();
  }

  private void Update()
  {
    HandleShooting();
  }
}
