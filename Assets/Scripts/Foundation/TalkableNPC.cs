using System;
using HeroesOfCrimson.Utils;
using UnityEngine;

[Serializable]
public class DialogueState
{
    public string DialogueId;
    public string StepId;
}

public class TalkableNPC : MonoBehaviour
{
    private LineRenderer _rangeCircle;

    public Player player;
    public float interactionRange = 3f;
    public Transform interactionImg;
    public Transform interactionPrompt;

    public int id = 1;
    public DialogueState dialogueState = new DialogueState();

    private void Start()
    {
        _rangeCircle = Utils.CreateCircle(
            transform,
            "InteractionRange",
            interactionRange,
            new Color(0.8f, 0f, 0f, 0.4f)
        );

        EnsureDialogueStateInitialized();
    }

    public void EnsureDialogueStateInitialized()
    {
        if (dialogueState != null &&
            !string.IsNullOrEmpty(dialogueState.DialogueId) &&
            !string.IsNullOrEmpty(dialogueState.StepId))
        {
            return;            
        }

        var ctrl = DialogueController.Singleton;
        if (ctrl == null)
        {
            Debug.LogError("DialogueController.Singleton is null (init order issue).");
            return;
        }

        var defaultDialogue = ctrl.GetDefaultDialogueForNpc(id);
        if (defaultDialogue == null)
        {
            Debug.LogError($"No dialogues found for NPC id {id}.");
            return;
        }

        if (dialogueState == null) dialogueState = new DialogueState();

        dialogueState.DialogueId = defaultDialogue.Id;
        dialogueState.StepId = defaultDialogue.StartStepId;
    }

    private void Update()
    {
        if (!player) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool isNear = distance <= interactionRange;

        if (isNear)
        {
            DialogueController.Singleton.CurrentNPC = this;
            DialogMenu.Singleton.CanBeOpened = true;
            if (interactionImg) interactionImg.localScale = Vector3.one;
            if (interactionPrompt) interactionPrompt.localScale = Vector3.one;
        }
        else
        {
            if (DialogueController.Singleton.CurrentNPC == this)
            {
                DialogueController.Singleton.CurrentNPC = null;
            }

            DialogMenu.Singleton.CanBeOpened = false;
            if (interactionImg) interactionImg.localScale = Vector3.zero;
            if (interactionPrompt) interactionPrompt.localScale = Vector3.zero;
        }
    }
}
