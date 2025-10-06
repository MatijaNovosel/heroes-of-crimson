using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
  public FloatingTextManager floatingTextManager;
  public DatabaseManager databaseManager;
  public static GameManager instance;

  private void Awake()
  {
    instance = this;
  }

  void Start()
  {
    //
  }

  void Update()
  {
    //
  }

  public void ShowText(
    string msg,
    int fontSize,
    Color color,
    Vector3 position,
    Vector3 motion,
    float duration
  )
  {
    floatingTextManager.Show(
      msg,
      fontSize,
      color,
      position,
      motion,
      duration
    );
  }
}
