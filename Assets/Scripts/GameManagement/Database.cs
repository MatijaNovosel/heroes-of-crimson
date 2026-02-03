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
    item.tag = (Constants.ItemTag)databaseItem.tag;

    switch (item.tag)
    {
      case Constants.ItemTag.Misc:
      {
        item.sprite = ResourceCacher.Singleton.MiscSprites.First(x => x.name == databaseItem.spritePath);
        break;
      }
      case Constants.ItemTag.Weapon:
      case Constants.ItemTag.Ability:
      case Constants.ItemTag.Armor:
      case Constants.ItemTag.Accessory:
      {
        item.sprite = ResourceCacher.Singleton.ArmorAndWeaponSprites.First(x => x.name == databaseItem.spritePath);
        break;
      }
      case Constants.ItemTag.Consumable:
      {
        item.sprite = ResourceCacher.Singleton.ConsumableSprites.First(x => x.name == databaseItem.spritePath);
        break;
      }
    }

    var projectileSprite = ResourceCacher.Singleton.ProjectileSprites.FirstOrDefault(x => x.name == databaseItem.projectilePath);
    
    item.projectileSprite = projectileSprite;
    
    item.minDamage = databaseItem.minDamage;
    item.maxDamage = databaseItem.maxDamage;
    item.rarity = (Constants.ItemRarity)databaseItem.rarity;
    item.stats = databaseItem.stats;
    item.impactColor = Color.white;
    
    item.projectileCount = databaseItem.projectileCount;
    item.projectileDegree = databaseItem.projectileDegree;
    item.range = databaseItem.range;
    item.projectileScale = databaseItem.projectileScale;

    if (databaseItem.impactColor != null)
    {
      item.impactColor = Utils.FromHex(databaseItem.impactColor);
    }
    
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
