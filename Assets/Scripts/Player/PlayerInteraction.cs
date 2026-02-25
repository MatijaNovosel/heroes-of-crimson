using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 3f;
    public LayerMask npcLayer;

    private TalkableNPC _current;
    private readonly Collider2D[] _hits = new Collider2D[32];

    private void Update()
    {
        if (DialogMenu.Singleton.DialogMenuOpen)
        {
            SetCurrent(null);
            return;
        }
        var best = FindBestNpc();
        SetCurrent(best);
    }

    private TalkableNPC FindBestNpc()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            interactionRange,
            _hits,
            npcLayer
        );

        TalkableNPC best = null;
        float bestDistSq = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (!col) continue;

            var npc = col.GetComponent<TalkableNPC>();
            if (!npc) continue;

            float distSq = (npc.transform.position - transform.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = npc;
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
}