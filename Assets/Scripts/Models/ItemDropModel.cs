using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public readonly struct ItemDropModel
    {
        public readonly int ItemId;
        public readonly float DropChance;
        public readonly bool? Guaranteed;
    
        public ItemDropModel(
            int itemId,
            float dropChance,
            bool? guaranteed = false
        )
        {
            this.ItemId = itemId;
            this.DropChance = dropChance;
            this.Guaranteed = guaranteed;
        }
    }
}
