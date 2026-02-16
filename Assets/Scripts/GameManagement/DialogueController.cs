// DialogueController.cs
using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models.Dialogue;
using UI.Inventory;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Singleton;
    public Inventory playerInventory;

    private DialogueModel _activeDialogue;
    private DialogueStepModel _currentStep;
    
    public DialogueOptions dialogueOptions;

    public TalkableNPC CurrentNPC;

    public Dictionary<int, List<DialogueModel>> dialoguesByNpc = new();
    public Dictionary<string, DialogueModel> dialoguesById = new();

    private const string DialogueResourcesFolder = "Misc/Dialogue";

    private void Awake()
    {
        Singleton = this;
        LoadDialogues();
    }

    public void LoadDialogues()
    {
        dialoguesByNpc = new Dictionary<int, List<DialogueModel>>();
        dialoguesById = new Dictionary<string, DialogueModel>();

        var files = Resources.LoadAll<TextAsset>(DialogueResourcesFolder);

        if (files == null || files.Length == 0)
        {
            Debug.LogError($"No dialogue json files found in Resources/{DialogueResourcesFolder}");
            return;
        }

        foreach (var file in files)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.text)) continue;

            DialogueFileDto parsed;
            
            try
            {
                parsed = JsonUtility.FromJson<DialogueFileDto>(file.text);
            }
            catch
            {
                Debug.LogError($"Failed to parse dialogue file: {file.name}");
                continue;
            }

            if (parsed?.dialogues == null) continue;

            foreach (var dto in parsed.dialogues)
            {
                var model = ToModel(dto);
                if (model == null) continue;

                if (!string.IsNullOrEmpty(model.Id))
                {
                    if (dialoguesById.ContainsKey(model.Id))
                    {
                        Debug.LogWarning($"Duplicate dialogue id '{model.Id}' in file '{file.name}'. Overwriting.");
                    }
                    dialoguesById[model.Id] = model;
                }

                if (!dialoguesByNpc.TryGetValue(model.NpcId, out var list))
                {
                    list = new List<DialogueModel>();
                    dialoguesByNpc.Add(model.NpcId, list);
                }
                list.Add(model);
            }
        }

        Debug.Log($"Loaded {dialoguesById.Count} dialogue(s) for {dialoguesByNpc.Count} NPC(s).");
    }

    private static DialogueModel ToModel(DialogueDto dto)
    {
        if (dto == null) return null;
        var stepsDict = new Dictionary<string, DialogueStepModel>();

        if (dto.steps != null)
        {
            foreach (var s in dto.steps)
            {
                if (s == null || string.IsNullOrEmpty(s.id)) continue;

                var step = new DialogueStepModel
                {
                    Id = s.id,
                    Text = s.text,
                    NextStepId = s.nextStepId,
                    Choices = new List<DialogueChoiceModel>()
                };

                if (s.choices != null)
                {
                    foreach (var c in s.choices.Where(c => c != null))
                    {
                        var triggers = new List<DialogueTriggerModel>();

                        if (c.triggers != null)
                        {
                            foreach (var t in c.triggers)
                            {
                                if (t == null) continue;
                                triggers.Add(new DialogueTriggerModel
                                {
                                    Id = t.id,
                                    Value = t.value
                                });
                            }
                        }

                        step.Choices.Add(new DialogueChoiceModel
                        {
                            Id = c.id,
                            Text = c.text,
                            NextStepId = c.nextStepId,
                            Triggers = triggers
                        });
                    }
                }
                stepsDict[step.Id] = step;
            }
        }

        return new DialogueModel
        {
            Id = dto.id,
            NpcId = dto.npcId,
            StartStepId = dto.startStepId,
            Steps = stepsDict
        };
    }

    public DialogueModel GetDefaultDialogueForNpc(int npcId)
    {
        if (!dialoguesByNpc.TryGetValue(npcId, out var list) || list == null || list.Count == 0)
        {
            return null;
        }
        return list[0];
    }

    public void StartDialogue(TalkableNPC npc)
    {
        if (npc == null)
        {
            Debug.LogError("StartDialogue called with null npc.");
            return;
        }

        CurrentNPC = npc;
        npc.EnsureDialogueStateInitialized();

        if (!dialoguesById.TryGetValue(npc.dialogueState.DialogueId, out _activeDialogue) || _activeDialogue == null)
        {
            Debug.LogError($"DialogueId '{npc.dialogueState.DialogueId}' not found for NPC {npc.id}.");
            return;
        }

        GoToStep(npc.dialogueState.StepId);
        dialogueOptions.Init(_currentStep.Choices);
    }
    
    private void _handleChoiceTriggers(List<DialogueTriggerModel> triggers)
    {
        foreach (var t in triggers)
        {
            switch (t.Id)
            {
                // 1 - Give item
                case (int)Constants.DialogueChoiceTriggers.GiveItem:
                    var item = Database.Singleton.GetItem(t.Value);
                    playerInventory.SpawnItem(item);
                    PlayerLog.Singleton.AddItem("You received <color=#F1C40F>White Monster Energy</color>!.");
                    PlayerLog.Singleton.AddItem("You feel very conflicted about your choices.");
                    break;
                default:
                    Debug.LogWarning($"Unknown trigger id={t.Id} value={t.Value}");
                    break;
            }
        }
    }

    public void ChooseOption(string choiceId)
    {
        if (_currentStep?.Choices == null) return;

        var choice = _currentStep.Choices.Find(x => x.Id == choiceId);
        if (choice == null) return;

        if (choice.Triggers != null && choice.Triggers.Count > 0)
        {
            _handleChoiceTriggers(choice.Triggers);
        }

        if (string.IsNullOrEmpty(choice.NextStepId))
        {
            DialogMenu.Singleton.CloseDialog();
            return;
        }

        GoToStep(choice.NextStepId);
    }

    private void GoToStep(string stepId)
    {
        if (string.IsNullOrEmpty(stepId)) return;

        if (_activeDialogue == null || _activeDialogue.Steps == null ||
            !_activeDialogue.Steps.TryGetValue(stepId, out var step))
        {
            Debug.LogError($"Dialogue step not found: {stepId} (Dialogue {_activeDialogue?.Id})");
            return;
        }

        _currentStep = step;

        if (CurrentNPC != null && CurrentNPC.dialogueState != null)
        {
            CurrentNPC.dialogueState.StepId = stepId;
        }

        DialogMenu.Singleton.UpdateText(_currentStep.Text);
        dialogueOptions.Init(_currentStep.Choices);
    }

    public DialogueStepModel GetCurrentStep() => _currentStep;
}
