using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models.Dialogue;
using Newtonsoft.Json;
using UI.Inventory;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Singleton;
    public Inventory playerInventory;
    public Player player;

    private DialogueModel _activeDialogue;
    private DialogueStepModel _currentStep;
    
    public DialogueOptions dialogueOptions;

    public TalkableNPC CurrentNPC;

    private Dictionary<int, List<DialogueModel>> _dialoguesByNpc = new();
    private Dictionary<string, DialogueModel> _dialoguesById = new();

    private const string DialogueResourcesFolder = "Misc/Dialogue";

    public MonoBehaviour gameStateSource;
    private IDialogueGameState _gameState;

    private void Awake()
    {
        Singleton = this;
        _gameState = gameStateSource as IDialogueGameState;
        if (_gameState == null)
        {
            Debug.LogError("DialogueController: gameStateSource must implement IDialogueGameState");
        }
        LoadDialogues();
    }

    public void LoadDialogues()
    {
        _dialoguesByNpc = new Dictionary<int, List<DialogueModel>>();
        _dialoguesById = new Dictionary<string, DialogueModel>();

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
                parsed = JsonConvert.DeserializeObject<DialogueFileDto>(file.text);
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
                    _dialoguesById[model.Id] = model;
                }

                if (!_dialoguesByNpc.TryGetValue(model.NpcId, out var list))
                {
                    list = new List<DialogueModel>();
                    _dialoguesByNpc.Add(model.NpcId, list);
                }
                list.Add(model);
            }
        }

        Debug.Log($"Loaded {_dialoguesById.Count} dialogue(s) for {_dialoguesByNpc.Count} NPC(s).");
    }

    private static object JTokenToClr(JToken t)
    {
        if (t == null || t.Type == JTokenType.Null) return null;

        return t.Type switch
        {
            JTokenType.Boolean => t.Value<bool>(),
            JTokenType.Integer => t.Value<int>(),
            JTokenType.Float => t.Value<float>(),
            JTokenType.String => t.Value<string>(),
            _ => t.ToString()
        };
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
                    foreach (var c in s.choices.Where(x => x != null))
                    {
                        var triggers = new List<DialogueTriggerModel>();
                        if (c.triggers != null)
                        {
                            foreach (var t in c.triggers)
                            {
                                if (t == null) continue;
                                triggers.Add(new DialogueTriggerModel
                                {
                                    Type = t.type,
                                    Key = t.key,
                                    Value = JTokenToClr(t.value)
                                });
                            }
                        }

                        var conditions = new List<DialogueConditionModel>();
                        if (c.conditions != null)
                        {
                            foreach (var cond in c.conditions)
                            {
                                if (cond == null) continue;
                                conditions.Add(new DialogueConditionModel
                                {
                                    Type = cond.type,
                                    Key = cond.key,
                                    Stat = cond.stat,
                                    Op = cond.op,
                                    Value = JTokenToClr(cond.value)
                                });
                            }
                        }

                        step.Choices.Add(new DialogueChoiceModel
                        {
                            Id = c.id,
                            Text = c.text,
                            NextStepId = c.nextStepId,
                            OnFailStepId = c.onFailStepId,
                            Conditions = conditions,
                            Triggers = triggers
                        });
                    }
                }
                stepsDict[step.Id] = step;
            }
        }

        var startPoints = new List<DialogueStartPointModel>();
        if (dto.startPoint != null)
        {
            foreach (var sp in dto.startPoint.Where(x => x != null))
            {
                var spConds = new List<DialogueConditionModel>();
                if (sp.conditions != null)
                {
                    foreach (var cond in sp.conditions.Where(x => x != null))
                    {
                        spConds.Add(new DialogueConditionModel
                        {
                            Type = cond.type,
                            Key = cond.key,
                            Stat = cond.stat,
                            Op = cond.op,
                            Value = JTokenToClr(cond.value)
                        });
                    }
                }

                startPoints.Add(new DialogueStartPointModel
                {
                    Conditions = spConds,
                    StartStepId = sp.startStepId
                });
            }
        }

        return new DialogueModel
        {
            Id = dto.id,
            NpcId = dto.npcId,
            StartStepId = dto.startStepId,
            StartPoints = startPoints,
            Steps = stepsDict
        };
    }

    public DialogueModel GetDefaultDialogueForNpc(int npcId)
    {
        if (!_dialoguesByNpc.TryGetValue(npcId, out var list) || list == null || list.Count == 0)
        {
            return null;
        }
        return list[0];
    }

    public void StartDialogue(TalkableNPC npc)
    {
        if (npc == null) return;

        CurrentNPC = npc;
        npc.EnsureDialogueStateInitialized();

        if (!_dialoguesById.TryGetValue(npc.dialogueState.DialogueId, out _activeDialogue) || _activeDialogue == null)
        {
            Debug.LogError($"DialogueId '{npc.dialogueState.DialogueId}' not found for NPC {npc.id}.");
            return;
        }

        var entryStepId = ResolveStartStepId(_activeDialogue);

        GoToStep(entryStepId);
        dialogueOptions.Init(_currentStep.Choices);
    }
    
    private void HandleChoiceTriggers(List<DialogueTriggerModel> triggers)
    {
        foreach (var t in triggers)
        {
            if (t == null) continue;

            switch (t.Type)
            {
                case Constants.DialogueTriggers.GiveItem:
                {
                    int itemId = t.Value is int i ? i : 0;
                    var item = Database.Singleton.GetItem(itemId);
                    playerInventory.SpawnItem(item);
                    PlayerLog.Singleton.AddItem("You received <color=#F1C40F>White Monster Energy</color>!");
                    PlayerLog.Singleton.AddItem("You feel very conflicted about your choices.");
                    break;
                }

                case Constants.DialogueTriggers.SetFlag:
                {
                    if (_gameState == null) break;
                    bool v = t.Value is bool b && b;
                    _gameState.SetFlag(t.Key, v);
                    break;
                }
                
                case Constants.DialogueTriggers.GiveXp:
                {
                    var xp = t.Value is int i ? i : 0;
                    player.GiveXp(xp);
                    break;
                }

                default:
                    Debug.LogWarning($"Unknown trigger type='{t.Type}' key='{t.Key}' value='{t.Value}'");
                    break;
            }
        }
    }

    public void ChooseOption(string choiceId)
    {
        if (_currentStep?.Choices == null) return;

        var choice = _currentStep.Choices.Find(x => x.Id == choiceId);
        if (choice == null) return;

        bool ok = AreConditionsMet(choice.Conditions);

        if (!ok)
        {
            if (!string.IsNullOrEmpty(choice.OnFailStepId))
            {
                GoToStep(choice.OnFailStepId);
            }
            else
            {
                DialogMenu.Singleton.CloseDialog();
            }
            return;
        }

        if (choice.Triggers != null && choice.Triggers.Count > 0)
        {
            HandleChoiceTriggers(choice.Triggers);
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

        if (
            _activeDialogue?.Steps == null ||
            !_activeDialogue.Steps.TryGetValue(stepId, out var step)
        )
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
    
    private bool AreConditionsMet(List<DialogueConditionModel> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;

        foreach (var c in conditions)
        {
            if (c == null) continue;
            if (!IsConditionMet(c)) return false;
        }
        return true;
    }

    private bool IsConditionMet(DialogueConditionModel c)
    {
        if (_gameState == null) return false;

        switch (c.Type)
        {
            case "flag":
            {
                var actual = _gameState.GetFlag(c.Key);
                var expected = c.Value is bool b ? b : false;
                return CompareBool(actual, c.Op, expected);
            }
            case "stat":
            {
                var actualStat = 0;

                switch ((Constants.Stats)c.Stat)
                {
                    case Constants.Stats.MGT:
                        actualStat = (int)player.actualMgt;
                        break;
                    case Constants.Stats.ARM:
                        actualStat = (int)player.actualArm;
                        break;
                    case Constants.Stats.WIS:
                        actualStat = (int)player.actualWis;
                        break;
                    case Constants.Stats.STR:
                        actualStat = (int)player.actualStr;
                        break;
                    case Constants.Stats.AGI:
                        actualStat = (int)player.actualAgi;
                        break;
                    case Constants.Stats.SWF:
                        actualStat = (int)player.actualSwf;
                        break;
                }
                
                var expected = c.Value is int i ? i : 0;
                return CompareInt(actualStat, c.Op, expected);
            }
            default:
                Debug.LogWarning($"Unknown condition type '{c.Type}'");
                return false;
        }
    }

    private static bool CompareBool(bool a, string op, bool b)
    {
        return op switch
        {
            "==" => a == b,
            "!=" => a != b,
            _ => false
        };
    }

    private static bool CompareInt(int a, string op, int b)
    {
        return op switch
        {
            "==" => a == b,
            "!=" => a != b,
            ">=" => a >= b,
            "<=" => a <= b,
            ">"  => a > b,
            "<"  => a < b,
            _ => false
        };
    }

    private string ResolveStartStepId(DialogueModel dialogue)
    {
        if (dialogue == null) return null;
        var result = dialogue.StartStepId;

        if (dialogue.StartPoints != null)
        {
            foreach (var sp in dialogue.StartPoints)
            {
                if (sp == null || string.IsNullOrEmpty(sp.StartStepId)) continue;
                if (AreConditionsMet(sp.Conditions)) return sp.StartStepId;
            }
        }
        
        return result;
    }

    public DialogueStepModel GetCurrentStep() => _currentStep;
}
