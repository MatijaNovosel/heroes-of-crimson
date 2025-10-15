using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models

{
    public readonly struct DamageModel
    {
        public readonly float Value;
        
        public DamageModel(
            float value
        )
        {
            this.Value = value;
        }
    }
}