using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using UnityEngine;

public class Database : MonoBehaviour
{
  public static Database Singleton;
  private DatabaseItemList _databaseItemList;
  private Sprite[] _armorAndWeaponSprites;

  private void Awake()
  {
    Singleton = this;
  }

  private void Start()
  {
    _armorAndWeaponSprites = Resources.LoadAll<Sprite>("Sprites/Items/armorAndWeapons");
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
    item.minDamage = databaseItem.minDamage;
    item.maxDamage = databaseItem.maxDamage;
    item.tag = (Constants.SlotTag)databaseItem.tag;
    item.projectileCount = databaseItem.projectileCount;
    
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
