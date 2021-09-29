using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  public float moveSpeed = 5f;
  public Animator animator;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  private void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
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

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(0, moveDelta.y),
      Mathf.Abs(moveDelta.y * Time.deltaTime),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(0, moveDelta.y * Time.deltaTime, 0);
    }

    hit = Physics2D.BoxCast(
      transform.position,
      boxCollider.size,
      0,
      new Vector2(moveDelta.x, 0),
      Mathf.Abs(moveDelta.x * Time.deltaTime),
      LayerMask.GetMask("Actor", "Blocking")
    );

    if (hit.collider == null)
    {
      transform.Translate(moveDelta.x * Time.deltaTime, 0, 0);
    }
  }
}
