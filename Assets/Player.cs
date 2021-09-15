using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  private Vector3 moveDelta;

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

    Debug.Log(x);
    Debug.Log(y);
  }
}
