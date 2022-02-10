using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
  public GameObject playerPrefab;

  private void Start()
  {
    PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(1.4f, 0.67f), Quaternion.identity);
  }
}
