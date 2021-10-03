using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDetector : MonoBehaviour
{
  private GameObject Player;
  public GameObject Projectile;

  // Start is called before the first frame update
  void Start()
  {
    Player = GameObject.Find("Player");
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
      Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

      Vector3 shootDirection = (new Vector3(worldPosition.x, worldPosition.y, 0) - Player.transform.position).normalized;

      GameObject bullet = Instantiate(Projectile, new Vector3(Player.transform.position.x, Player.transform.position.y, 0), Quaternion.identity);
      bullet.GetComponent<Projectile>().Setup(shootDirection);
    }
  }
}
