using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText
{
  public bool active;
  public GameObject obj;
  public TMP_Text text;
  public Vector3 motion;
  public float duration;
  private float lastShown;
  private float moveSpeed = 6f;

  public void Show()
  {
    active = true;
    lastShown = Time.time;
    obj.SetActive(active);
  }

  public void Hide()
  {
    active = false;
    obj.SetActive(active);
  }

  public void UpdateFloatingText()
  {
    if (!active) return;

    if (Time.time - lastShown > duration)
    {
      Hide();
    }

    obj.transform.position += motion * (moveSpeed * Time.deltaTime);
  }
}
