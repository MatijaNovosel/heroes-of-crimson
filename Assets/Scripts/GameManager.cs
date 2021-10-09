using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public FloatingTextManager floatingTextManager;
  private Database database;
  public static GameManager instance;

  private void Awake()
  {
    database = new Database();
    database.Seed();
    instance = this;
  }

  void Start()
  {

  }

  void Update()
  {

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
