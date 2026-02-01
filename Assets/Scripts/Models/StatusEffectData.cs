using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public struct StatusEffectData
    {
        public string Name;
        public string Description;
        public bool IsNegative;
        
        public StatusEffectData(
            string name,
            string description,
            bool isNegative
        )
        {
            this.Name = name;
            this.Description = description;
            this.IsNegative = isNegative;
        }
    }
}