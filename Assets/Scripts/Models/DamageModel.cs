using UnityEngine;

namespace Models

{
    public readonly struct DamageModel
    {
        public GameObject Source { get; }
        public readonly float Value;
        public readonly string Destination;
        
        public DamageModel(GameObject source, float value, string destination)
        {
            this.Value = value;
            this.Source = source;
            this.Destination = destination;
        }
    }
}