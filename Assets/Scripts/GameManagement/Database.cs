using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class Database : MonoBehaviour
{
  public static Database Singleton;
  private DatabaseItemList _databaseItemList;
  private Sprite[] _armorAndWeaponSprites;
  private Sprite[] _projectileSprites;

  private void Awake()
  {
    Singleton = this;
    _armorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
    _projectileSprites = Resources.LoadAll<Sprite>("Sprites/Projectiles/projectiles");
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
    item.sprite = _armorAndWeaponSprites.First(x => x.name == databaseItem.spritePath);
    item.projectileSprite = _projectileSprites.First(x => x.name == databaseItem.projectilePath);
    item.minDamage = databaseItem.minDamage;
    item.maxDamage = databaseItem.maxDamage;
    item.tag = (Constants.SlotTag)databaseItem.tag;
    item.rarity = (Constants.ItemRarity)databaseItem.rarity;
    item.projectileCount = databaseItem.projectileCount;
    item.stats = databaseItem.stats;
    item.projectileDegree = databaseItem.projectileDegree;
    
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
