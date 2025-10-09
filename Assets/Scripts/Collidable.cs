using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collidable : MonoBehaviour
{
  public ContactFilter2D filter;
  private BoxCollider2D boxCollider;
  private Collider2D[] hits = new Collider2D[10];

  protected virtual void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
  }

  protected virtual void Update()
  {
    boxCollider.Overlap(filter, hits);
    for (var i = 0; i < hits.Length; i++)
    {
      if (hits[i] is null) continue;
      OnCollide(hits[i]);
      hits[i] = null;
    }
  }

  protected virtual void OnCollide(Collider2D collider)
  {
    //
  }
}
