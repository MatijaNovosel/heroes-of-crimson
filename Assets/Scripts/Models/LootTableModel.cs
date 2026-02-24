using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public readonly struct LootTableModel
    {
        public readonly Constants.LootTableEnum Id;
        public readonly List<ItemDropModel> Items;
    
        public LootTableModel(
            Constants.LootTableEnum id,
            List<ItemDropModel> items
        )
        {
            this.Id = id;
            this.Items = items;
        }
    }
}
