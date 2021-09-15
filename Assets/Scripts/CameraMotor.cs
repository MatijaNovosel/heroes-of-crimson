using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
  public Transform lookAt;
  public float boundX = 0.15f;
  public float boundY = 0.05f;

  // Update is called once per frame
  private void LateUpdate()
  {
    Vector3 delta = Vector3.zero;
    float deltaX = lookAt.position.x - transform.position.x;
    float deltaY = lookAt.position.y - transform.position.y;

    // Check if inside bound of X axis
    if (deltaX > boundX || deltaX < -boundX)
    {
      if (transform.position.x < lookAt.position.x)
      {
        delta.x = deltaX - boundX;
      }
      else
      {
        delta.x = deltaX + boundX;
      }
    }

    // Check if inside bound of Y axis
    if (deltaY > boundY || deltaY < -boundY)
    {
      if (transform.position.y < lookAt.position.y)
      {
        delta.y = deltaY - boundX;
      }
      else
      {
        delta.y = deltaY + boundX;
      }
    }

    transform.position += new Vector3(delta.x, delta.y, 0);
  }
}
