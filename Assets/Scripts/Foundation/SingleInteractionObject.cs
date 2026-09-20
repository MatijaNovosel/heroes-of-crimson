using HeroesOfCrimson.Utils;
using UnityEngine;

public class SingleInteractionObject : MonoBehaviour
{
    public Transform interactionImg;
    private Transform _interactionPrompt;
    public Player player;
    public string trigger = "teleportToMarker";
    public int value;
    
    void Start()
    {
        ShowPrompt(false);
        _interactionPrompt = GameObject.Find("InteractionPrompts").GetComponent<Transform>();
    }

    public void Interact()
    {
        switch (trigger)
        {
            case Constants.DialogueTriggers.TeleportToMarker:
            {
                var markerId = (Constants.TeleportMarkers)value;

                if (markerId == Constants.TeleportMarkers.Dungeon)
                {
                    var dungeonBegun = DungeonGenerator.Singleton.BeginDungeonRun();
                    if (!dungeonBegun) return;
                }

                player.TeleportToMarker(markerId);
                break;
            }

            case Constants.DialogueTriggers.CompleteDungeon:
                if (DungeonGenerator.Singleton != null)
                {
                    DungeonGenerator.Singleton.CompleteDungeon(player);
                }
                break;
        }
    }
    
    public void ShowPrompt(bool show)
    {
        if (interactionImg) interactionImg.localScale = show ? Vector3.one : Vector3.zero;
        if (_interactionPrompt) _interactionPrompt.localScale = show ? Vector3.one : Vector3.zero;
    }
}