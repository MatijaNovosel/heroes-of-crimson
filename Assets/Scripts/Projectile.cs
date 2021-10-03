using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  private Vector3 direction;

  // Start is called before the first frame update
  public void Setup(Vector3 direction)
  {
    this.direction = direction;
  }

  // Update is called once per frame
  void Update()
  {
    float moveSpeed = 30f;
    transform.position += direction * moveSpeed * Time.deltaTime;
  }
}
