using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class DialogueState
{
    public string DialogueId;
    public string StepId;
}

public class TalkableNPC : MonoBehaviour
{
    public Transform interactionImg;
    public Transform interactionPrompt;

    public string npcName;
    public Sprite portraitImg;

    public int id = 1;
    public DialogueState dialogueState = new DialogueState();

    private void Start()
    {
        EnsureDialogueStateInitialized();
        ShowPrompt(false);
    }

    public void ShowPrompt(bool show)
    {
        if (interactionImg) interactionImg.localScale = show ? Vector3.one : Vector3.zero;
        if (interactionPrompt) interactionPrompt.localScale = show ? Vector3.one : Vector3.zero;
    }

    public void EnsureDialogueStateInitialized()
    {
        if (dialogueState != null &&
            !string.IsNullOrEmpty(dialogueState.DialogueId) &&
            !string.IsNullOrEmpty(dialogueState.StepId)
        )
        {
            return;
        }

        var ctrl = DialogueController.Singleton;
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
}