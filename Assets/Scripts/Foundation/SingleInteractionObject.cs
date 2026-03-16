using HeroesOfCrimson.Utils;
using UnityEngine;

public class SingleInteractionObject : MonoBehaviour
{
    public Transform interactionImg;
    public Transform interactionPrompt;
    public Player player;
    public string trigger = "teleportToMarker";
    public int value;
    
    void Start()
    {
        ShowPrompt(false);
    }

    public void Interact()
    {
        switch (trigger)
        {
            case Constants.DialogueTriggers.TeleportToMarker:
                player.TeleportToMarker((Constants.TeleportMarkers)value);
                break;
        }
    }
    
    public void ShowPrompt(bool show)
    {
        if (interactionImg) interactionImg.localScale = show ? Vector3.one : Vector3.zero;
        if (interactionPrompt) interactionPrompt.localScale = show ? Vector3.one : Vector3.zero;
    }
}
