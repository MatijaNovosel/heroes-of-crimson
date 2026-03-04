using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public Player player;
    public float interactionRange = 3f;

    void Update()
    {
        if (!player) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool isNear = distance <= interactionRange;

        if (isNear)
        {
            player.Kill();
        }
    }
}
