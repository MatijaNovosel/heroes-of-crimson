using System.Linq;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class Database : MonoBehaviour
{
  public static Database Singleton;
  private DatabaseItemList _databaseItemList;

  private void Awake()
  {
    Singleton = this;
    _loadItems();
  }

  public Item GetItem(int id)
  {
    var databaseItem = _databaseItemList.items.FirstOrDefault(x => x.id == id);

    // TODO: Handle this better
    if (databaseItem is null) return null;
    
    var item = ScriptableObject.CreateInstance<Item>();
    
    item.id = databaseItem.id;
    item.name = databaseItem.name;
    item.description = databaseItem.description;
    item.sprite = ResourceCacher.Singleton.ArmorAndWeaponSprites.First(x => x.name == databaseItem.spritePath);

    var projectileSprite = ResourceCacher.Singleton.ProjectileSprites.FirstOrDefault(x => x.name == databaseItem.projectilePath);
    
    item.projectileSprite = projectileSprite;
    
    item.minDamage = databaseItem.minDamage;
    item.maxDamage = databaseItem.maxDamage;
    item.tag = (Constants.SlotTag)databaseItem.tag;
    item.rarity = (Constants.ItemRarity)databaseItem.rarity;
    item.projectileCount = databaseItem.projectileCount;
    item.stats = databaseItem.stats;
    item.projectileDegree = databaseItem.projectileDegree;
    item.shootSound = (Constants.Sounds)databaseItem.shootSound;
    
    return item;
  }

  private void _loadItems()
  {
    var jsonFile = Resources.Load<TextAsset>("Misc/Items");
    
    if (!jsonFile)
    {
      Debug.LogError("Item JSON not found!");
      return;
    }
    
    _databaseItemList = JsonUtility.FromJson<DatabaseItemList>(jsonFile.text);
  }
}
