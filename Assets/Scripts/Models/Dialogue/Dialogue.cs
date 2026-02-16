using System.Collections.Generic;

namespace Models.Dialogue
{
    public class DialogueModel
    {
        public string Id;
        public int NpcId;
        public string StartStepId;
        public Dictionary<string, DialogueStepModel> Steps;
    }

    public class DialogueStepModel
    {
        public string Id;
        public string Text;
        public List<DialogueChoiceModel> Choices;
        public string NextStepId;
    }

    public class DialogueTriggerModel
    {
        public int Id;
        public int Value;
    }

    public class DialogueChoiceModel
    {
        public string Id;
        public string Text;
        public string NextStepId;
        public List<DialogueTriggerModel> Triggers;
    }
}