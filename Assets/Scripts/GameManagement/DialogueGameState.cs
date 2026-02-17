using System.Collections.Generic;
using Models.Dialogue;
using UnityEngine;

namespace GameManagement
{
    public class DialogueGameState : MonoBehaviour, IDialogueGameState
    {
        private readonly Dictionary<string, bool> _flags = new();

        public bool GetFlag(string key) => key != null && _flags.TryGetValue(key, out var v) && v;
        public void SetFlag(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _flags[key] = value;
        }
    }
}