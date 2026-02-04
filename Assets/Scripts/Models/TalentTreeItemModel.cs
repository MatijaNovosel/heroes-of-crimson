using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public struct TalentTreeItemModel
    {
        public string Name;
        public string Description;
        
        public TalentTreeItemModel(
            string name,
            string description
        )
        {
            this.Name = name;
            this.Description = description;
        }
    }
}