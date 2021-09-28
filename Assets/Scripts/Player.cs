using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  private SpriteRenderer spriteRenderer;
  private List<KeyCode> keys;

  public Sprite upSprite;
  public Sprite downSprite;
  public Sprite stillSprite;

  private Vector3 moveDelta;
  private RaycastHit2D hit;

  private void Start()
  {
    keys = new List<KeyCode>();
    keys.Add(KeyCode.W);
    keys.Add(KeyCode.S);
    keys.Add(KeyCode.A);
    keys.Add(KeyCode.D);
    boxCollider = GetComponent<BoxCollider2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  private void FixedUpdate()
  {
    float x = Input.GetAxisRaw("Horizontal");
    float y = Input.GetAxisRaw("Vertical");

    if (Input.GetKey(KeyCode.W))
    {
      spriteRenderer.sprite = upSprite;
    }

    if (Input.GetKey(KeyCode.S))
    {
      spriteRenderer.sprite = downSprite;
    }

    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
    {
      spriteRenderer.sprite = stillSprite;
    }

    // Reset moveDelta
    moveDelta = new Vector3(x, y, 0);

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
