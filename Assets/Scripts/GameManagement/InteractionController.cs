using System;
using UI;
using UnityEngine;

namespace GameManagement
{
    public class InteractionController : MonoBehaviour
    {
        public static InteractionController Singleton;
        public SingleInteractionObject CurrentInteractionObject;
        
        private void HandleUIKeys()
        {
            if (Input.GetKeyDown(KeyCode.V) && CurrentInteractionObject)
            {
                CurrentInteractionObject.Interact();
            }
        }
        
        private void Awake()
        {
            Singleton = this;
        }
        
        private void Update()
        {
            HandleUIKeys();
        }
    }
}