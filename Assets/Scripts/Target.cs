using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
  private static List<Target> targets;

  // Start is called before the first frame update
  void Start()
  {

  }

  public Vector3 GetPosition()
  {
    return transform.position;
  }

  public static Target GetClosest(Vector3 position, float maxRange)
  {
    Target closest = null;
    foreach (Target target in targets)
    {
      if (Vector3.Distance(position, target.GetPosition()) <= maxRange)
      {
        if (closest == null)
        {
          closest = target;
        }
        else
        {
          if (Vector3.Distance(position, target.GetPosition()) < Vector3.Distance(position, closest.GetPosition()))
          {
            closest = target;
          }
        }
      }
    }
    return closest;
  }

  private void Awake()
  {
    if (targets == null)
    {
      targets = new List<Target>();
    }
    targets.Add(this);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
