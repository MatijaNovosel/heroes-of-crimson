using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database
{
  private List<Item> Items;

  public Database()
  {
    Items = new List<Item>();
  }

  Item GetItem(int id)
  {
    return Items.Find(item => item.Id == id);
  }

  public void Seed()
  {
    Items.Add(new Item()
    {
      Id = 1,
      Name = "Iron dagger",
      Description = "An iron dagger.",
      SpriteName = "sprWeapons1",
      Type = ItemType.WEAPON
    });
  }
}
