using GameManagement;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 2f;
    public LayerMask npcLayer;

    private TalkableNPC _current;
    private SingleInteractionObject _singleInteractionObject;
    private readonly Collider2D[] _hits = new Collider2D[32];

    private void Update()
    {
        if (DialogMenu.Singleton.DialogMenuOpen)
        {
            SetCurrent(null);
            return;
        }

        var bestNpc = FindClosestComponent<TalkableNPC>();
        var bestInteraction = FindClosestComponent<SingleInteractionObject>();

        SetCurrent(bestNpc);
        SetCurrentInteractionObject(bestInteraction);
    }

    private T FindClosestComponent<T>() where T : Component
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            interactionRange,
            _hits,
            npcLayer
        );

        T best = null;
        float bestDistSq = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (!col) continue;

            var comp = col.GetComponent<T>();
            if (!comp) continue;

            float distSq = (comp.transform.position - transform.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = comp;
            }
        }

        return best;
    }

    private void SetCurrent(TalkableNPC npc)
    {
        if (_current == npc) return;
        if (_current) _current.ShowPrompt(false);
        _current = npc;
        if (_current) _current.ShowPrompt(true);

        DialogueController.Singleton.CurrentNPC = _current;
        DialogMenu.Singleton.CanBeOpened = _current != null;
    }

    private void SetCurrentInteractionObject(SingleInteractionObject obj)
    {
        if (_singleInteractionObject == obj) return;
        if (_singleInteractionObject) _singleInteractionObject.ShowPrompt(false);
        _singleInteractionObject = obj;
        if (_singleInteractionObject) _singleInteractionObject.ShowPrompt(true);
        InteractionController.Singleton.CurrentInteractionObject = _singleInteractionObject;
    }
}