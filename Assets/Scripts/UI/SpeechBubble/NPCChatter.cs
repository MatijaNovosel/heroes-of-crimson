using System.Collections.Generic;
using UnityEngine;

public class NPCChatter : MonoBehaviour
{
    [Header("Idle Lines")]
    [TextArea(1, 3)]
    [SerializeField] private List<string> lines = new();
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private float hearingDistance = 8f;

    [Header("Hurt Lines")]
    [TextArea(1, 3)]
    [SerializeField] private List<string> hurtLines = new();
    [Range(0f, 1f)]
    [SerializeField] private float hurtChance = 0.25f;
    [SerializeField] private float hurtCooldown = 4f;

    [SerializeField] private bool writeToPlayerLog;
    [SerializeField] private string speakerName;

    private BaseNPCBehaviour _npc;
    private float _nextIdleTime;
    private float _nextHurtTime;
    private float _lastHp;
    private int _lastLineIndex = -1;
    private int _lastHurtLineIndex = -1;

    private void Start()
    {
        _npc = GetComponent<BaseNPCBehaviour>();
        if (_npc) _lastHp = _npc.hp;
        _nextIdleTime = Time.time + Random.Range(minInterval * 0.5f, maxInterval);
    }

    private void Update()
    {
        CheckHurt();

        if (Time.time < _nextIdleTime) return;
        _nextIdleTime = Time.time + Random.Range(minInterval, maxInterval);

        if (lines.Count == 0 || Random.value > chance || !PlayerIsNearby()) return;

        Say(PickLine(lines, ref _lastLineIndex));
    }

    private void CheckHurt()
    {
        if (!_npc) return;

        bool tookDamage = _npc.hp < _lastHp;
        _lastHp = _npc.hp;

        if (!tookDamage || hurtLines.Count == 0 || Time.time < _nextHurtTime) return;
        if (Random.value > hurtChance) return;

        _nextHurtTime = Time.time + hurtCooldown;
        Say(PickLine(hurtLines, ref _lastHurtLineIndex));
    }
    
    public void Say(string line)
    {
        SpeechBubbles.Say(transform, line);

        if (writeToPlayerLog && PlayerLog.Singleton)
        {
            PlayerLog.Singleton.AddItem(string.IsNullOrEmpty(speakerName) ? line : $"{speakerName}: {line}");
        }
    }

    private static string PickLine(List<string> options, ref int lastIndex)
    {
        if (options.Count == 1) return options[0];

        // Avoid saying the same line twice in a row
        int index;
        do index = Random.Range(0, options.Count);
        while (index == lastIndex);

        lastIndex = index;
        return options[index];
    }

    private bool PlayerIsNearby()
    {
        if (!Player.Singleton) return false;
        return Vector2.Distance(transform.position, Player.Singleton.transform.position) <= hearingDistance;
    }
}
