using System;
using System.Collections.Generic;

[Serializable]
public class DialogueFileDto
{
    public List<DialogueDto> dialogues;
}

[Serializable]
public class DialogueDto
{
    public string id;
    public int npcId;
    public string startStepId;
    public List<DialogueStepDto> steps;
}

[Serializable]
public class DialogueStepDto
{
    public string id;
    public string nextStepId;
    public string text;
    public List<DialogueChoiceDto> choices;
}

[Serializable]
public class DialogueTriggerDto
{
    public int id;
    public int value;
}

[Serializable]
public class DialogueChoiceDto
{
    public string id;
    public string text;
    public string nextStepId;
    public List<DialogueTriggerDto> triggers;
}