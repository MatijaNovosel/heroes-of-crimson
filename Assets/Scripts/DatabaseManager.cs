using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
  public Database db;
  
  void Start()
  {
    db = new Database();
    db.Seed();
  }
}
