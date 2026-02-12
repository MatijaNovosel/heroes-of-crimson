using System.Collections.Generic;

namespace Models.Dialogue
{
    public class DialogueModel
    {
        public int Id;
        public int StartStepId;
        public Dictionary<int, DialogueStepModel> Steps;
    }

    public class DialogueStepModel
    {
        public int Id;
        public string Text;
        public List<DialogueChoiceModel> Choices;
        public int? NextStepId;
    }

    public class DialogueChoiceModel
    {
        public int Id;
        public string Text;
        public int? NextStepId;
    }
}