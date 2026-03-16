using UnityEngine;
using UnityEngine.UI;
using System;

public class TrapdoorNPC : TalkableNPC
{
    public Sprite openSprite;
    public Sprite closedSprite;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var hasFlag = DialogueController.Singleton.HasStateFlag("touchedObelisk");
        _spriteRenderer.sprite = hasFlag ? openSprite : closedSprite;
    }
}