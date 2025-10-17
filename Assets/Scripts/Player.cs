using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine.UI;

[RequireComponent(typeof(BaseNPCBehaviour))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  private readonly float moveSpeed = 130f;
  public Animator animator;
  private AnimatorOverrideController animatorOverrideController;
  private BaseNPCBehaviour baseNPCBehaviour;
  public HealthBar healthBar;
  public Image abilityImage;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  public GameObject Projectile;
  private float lastFired;
  private float abilityUsedLast;
  private readonly float delay = 0.3f; // 0.8f -> 200 ms
  private readonly float abilityDelay = 1;
  private float abilityCooldownTimer = 0;
  private bool isShooting = false;

  bool CanFire()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - lastFired > delay;
  }

  bool CanCastAbility()
  {
    // Current game time in seconds - last time fired in game seconds
    return Time.time - abilityUsedLast > abilityDelay;
  }

  void Fire()
  {
    var shootDirection = (Utils.GetMousePosition() - gameObject.transform.position).normalized;
    var angle = Utils.GetAngleFromShootDirection(shootDirection);


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
      case < 135 and > 45:
        // Up
        animator.SetFloat("MouseDir", (int)Constants.ShootingMouseDirs.UP);
        animator.SetFloat("IdleState", 1);
        break;
      case > -45 and < 45:
        // Right
        animator.SetFloat("IdleState", 0);
        transform.localScale = Vector3.one;
        animator.SetFloat("MouseDir", (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      case > 135 and < 180 or > -180 and < -135:
        // Left
        animator.SetFloat("IdleState", 0);
        transform.localScale = new Vector3(-1, 1, 1);
        animator.SetFloat("MouseDir", (int)Constants.ShootingMouseDirs.HORIZONTAL);
        break;
      default:
        // Down
        animator.SetFloat("IdleState", 2);
        animator.SetFloat("MouseDir", (int)Constants.ShootingMouseDirs.DOWN);
        break;
    }

    var proj = Instantiate(
      Projectile,
      new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0),
      Quaternion.identity
    );

    proj.GetComponent<Projectile>().Setup(new ProjectileSetupModel(
      shootDirection,
      null,
      null,
      null,
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Enemy },
      new List<Constants.CollisionGroups> { Constants.CollisionGroups.Player }
    ));
    
    lastFired = Time.time;
  }

  private void HandleMoving()
  {
    var x = Input.GetAxisRaw("Horizontal");
    var y = Input.GetAxisRaw("Vertical");
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

    var speed = Utils.CalculatePlayerMovementSpeed(moveSpeed);
    LayerMask mask = LayerMask.GetMask("Actor", "Blocking", "NPC");

    var moveY = new Vector2(0, moveDelta.y);
    
    if (!Physics2D.BoxCast(
      transform.position,
      boxCollider.size, 
      0, 
      moveY, 
      Mathf.Abs(moveY.y * speed), mask)
    )
    {
      transform.Translate(0, moveY.y * speed, 0);
    }

    var moveX = new Vector2(moveDelta.x, 0);
    
    if (!Physics2D.BoxCast(
      transform.position,
      boxCollider.size, 
      0, 
      moveX, 
      Mathf.Abs(moveX.x * speed), mask)
    )
    {
      transform.Translate(moveX.x * speed, 0, 0);
    }
  }

  private void HandleShooting()
  {
    if (Input.GetMouseButton(0) && CanFire())
    {
      isShooting = true;
      animator.SetBool("Shooting", isShooting);
      animator.SetTrigger("Shoot");
      Fire();
    }

    if (!Input.GetMouseButtonUp(0)) return;
    
    isShooting = false;
    animator.SetBool("Shooting", isShooting);
  }

  private void HandleAbilityCooldown()
  {
    abilityCooldownTimer -= Time.deltaTime;

    if (abilityCooldownTimer < 0.0f)
    {
      abilityImage.fillAmount = 1.0f;
    }
    else
    {
      abilityImage.fillAmount = abilityCooldownTimer / abilityDelay;
    }
  }

  private void HandleAbility()
  {
    HandleAbilityCooldown();
    
    if (!Input.GetKey(KeyCode.R) || !CanCastAbility()) return;
    
    var cursorPosition = Utils.GetMousePosition();
    var meteorPrefab = Resources.Load<GameObject>("Prefabs/Meteor");

    if (meteorPrefab is null) return;

    abilityCooldownTimer = abilityDelay;

    var meteor = Instantiate(
      meteorPrefab,
      new Vector3(cursorPosition.x, cursorPosition.y + 0.8f, 0),
      Quaternion.identity
    );

    meteor.GetComponent<Meteor>().Setup(cursorPosition);

    abilityUsedLast = Time.time;
  }

  private void Start()
  {
    baseNPCBehaviour = GetComponent<BaseNPCBehaviour>();
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
    HandleAbility();
    HandleShooting();
  }
}
