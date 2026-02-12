using System;
using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using TMPro;
using UnityEngine;

public class PlayerLog : MonoBehaviour
{
    public static PlayerLog Singleton;
    private Dictionary<int, PlayerLogItem> _items = new();
    public GameObject playerLogItemPrefab;
    public RectTransform playerLogItemContainer;
    private int i = 0;
    
    void Start()
    {
        Singleton = this;
    }

    public void AddItem(string value)
    {
        var obj = Instantiate(playerLogItemPrefab, playerLogItemContainer.transform);
        var item = obj.GetComponent<PlayerLogItem>();
        item.Initialize(value, ++i);
        _items.Add(i, item);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AddItem("Hey look over here! <i> A man shouts at you from the distance </i>");
        }
    }
}
