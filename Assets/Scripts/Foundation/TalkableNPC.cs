using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.UI;

public class TalkableNPC : MonoBehaviour
{
    private LineRenderer _rangeCircle;
    public Player player;
    public float InteractionRange = 3f;
    public Transform interactionImg;
    public Transform interactionPrompt;
    
    void Start()
    {
        _rangeCircle = Utils.CreateCircle(
            transform,
            "InteractionRange",
            InteractionRange,
            new Color(0.8f, 0f, 0f, 0.4f)
        );
    }

    void Update()
    {
        if (!player) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool isNear = distance <= InteractionRange;

        if (isNear)
        {
            interactionImg.localScale = Vector3.one;
            interactionPrompt.localScale = Vector3.one;
        }
        else
        {
            interactionImg.localScale = Vector3.zero;
            interactionPrompt.localScale = Vector3.zero;
        }
    }
}
