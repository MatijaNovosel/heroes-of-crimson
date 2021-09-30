using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

  private float CalculateMovementSpeed()
  {
    /* 

      0.02 - Minimum
      0.05 - Maximum
      [1, 75] SPD
      [0.02, 0.05] RANGE

    */
    return 0.02f + (moveSpeed * 4.054054054054054e-4f);
  }

  private void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
    animator.runtimeAnimatorController = animatorOverrideController;
  }

  private void FixedUpdate()
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
      Mathf.Abs(moveDelta.y * CalculateMovementSpeed()),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(0, moveDelta.y * CalculateMovementSpeed(), 0);
    }

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(moveDelta.x, 0),
      Mathf.Abs(moveDelta.x * CalculateMovementSpeed()),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(moveDelta.x * CalculateMovementSpeed(), 0, 0);
    }
  }
}
