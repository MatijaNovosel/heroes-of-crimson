using System.Collections.Generic;
using System.Linq;
using GameManagement;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class Database : MonoBehaviour
{
  public static Database Singleton;
  private DatabaseItemList _databaseItemList;
  private Dictionary<Constants.ItemTag, Dictionary<string, Sprite>> _spriteLookup;

  private Dictionary<string, Sprite> ToDict(IEnumerable<Sprite> sprites)
  {
    return sprites.ToDictionary(s => s.name, s => s);
  }
  
  private void Awake()
  {
    Singleton = this;

    _spriteLookup = new()
    {
      { Constants.ItemTag.Misc, ToDict(ResourceCacher.Singleton.MiscSprites) },
      { Constants.ItemTag.Weapon, ToDict(ResourceCacher.Singleton.WeaponSprites) },
      { Constants.ItemTag.Armor, ToDict(ResourceCacher.Singleton.ArmorSprites) },
      { Constants.ItemTag.Accessory, ToDict(ResourceCacher.Singleton.AccessorySprites) },
      { Constants.ItemTag.Consumable, ToDict(ResourceCacher.Singleton.ConsumableSprites) }
    };

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

    item.sprite = _spriteLookup[item.tag]
      .FirstOrDefault(s => s.Value.name == databaseItem.spritePath).Value;

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
  
  private void LoadAndAdd(string path)
  {
    var file = Resources.Load<TextAsset>(path);
    if (file == null)
    {
      Debug.LogError($"Item database file not found at {path}");
      return;
    }

    var list = JsonUtility.FromJson<DatabaseItemList>(file.text);
    if (list?.items == null) return;

    _databaseItemList.items.AddRange(list.items);
  }
  
  private void _loadItems()
  {
    _databaseItemList = new DatabaseItemList
    {
      items = new List<DatabaseItem>()
    };
    
    LoadAndAdd("Misc/Items/Weapons");
    LoadAndAdd("Misc/Items/Accessories");
    LoadAndAdd("Misc/Items/Misc");
    LoadAndAdd("Misc/Items/Armor");
    LoadAndAdd("Misc/Items/Consumables");
  }
}
