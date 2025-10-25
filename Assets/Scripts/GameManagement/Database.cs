using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Database
{
  readonly private List<Item> Items;

  public Database()
  {
    Items = new List<Item>();
  }

  Item GetItem(int id)
  {
    return Items.First();
  }

  public void Seed()
  {
    //
  }
}
