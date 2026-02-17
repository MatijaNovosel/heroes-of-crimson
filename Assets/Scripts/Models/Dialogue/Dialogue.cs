using System.Collections.Generic;
using UnityEngine;

namespace Models.Dialogue
{
    public class DialogueModel
    {
        public string Id;
        public int NpcId;
        public string StartStepId;
        public List<DialogueStartPointModel> StartPoints;

        public Dictionary<string, DialogueStepModel> Steps;
    }

    public class DialogueStartPointModel
    {
        public List<DialogueConditionModel> Conditions;
        public string StartStepId;
    }

    public class DialogueStepModel
    {
        public string Id;
        public string Text;
        public List<DialogueChoiceModel> Choices;
        public string NextStepId;
    }

    public class DialogueChoiceModel
    {
        public string Id;
        public string Text;
        public string NextStepId;

        public string OnFailStepId;
        public List<DialogueConditionModel> Conditions;

        public List<DialogueTriggerModel> Triggers;
    }

    public class DialogueConditionModel
    {
        public string Type;
        public string Key; 
        public int Stat;
        public string Op;
        public object Value;
    }

    public class DialogueTriggerModel
    {
        public string Type;
        public string Key;
        public object Value;
    }
    
    public interface IDialogueGameState
    {
        bool GetFlag(string key);
        void SetFlag(string key, bool value);
    }
}