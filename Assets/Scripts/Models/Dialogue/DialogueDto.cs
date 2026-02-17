using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class DialogueFileDto
{
    [JsonProperty("dialogues")]
    public List<DialogueDto> dialogues;
}

[Serializable]
public class DialogueDto
{
    [JsonProperty("id")]
    public string id;

    [JsonProperty("npcId")]
    public int npcId;

    [JsonProperty("startStepId")]
    public string startStepId;

    [JsonProperty("startPoint")]
    public List<DialogueStartPointDto> startPoint;

    [JsonProperty("steps")]
    public List<DialogueStepDto> steps;
}

[Serializable]
public class DialogueStartPointDto
{
    [JsonProperty("conditions")]
    public List<DialogueConditionDto> conditions;

    [JsonProperty("startStepId")]
    public string startStepId;
}

[Serializable]
public class DialogueStepDto
{
    [JsonProperty("id")]
    public string id;

    [JsonProperty("nextStepId")]
    public string nextStepId;

    [JsonProperty("text")]
    public string text;

    [JsonProperty("choices")]
    public List<DialogueChoiceDto> choices;
}

[Serializable]
public class DialogueChoiceDto
{
    [JsonProperty("id")]
    public string id;

    [JsonProperty("text")]
    public string text;

    [JsonProperty("nextStepId")]
    public string nextStepId;

    [JsonProperty("onFailStepId")]
    public string onFailStepId;

    [JsonProperty("conditions")]
    public List<DialogueConditionDto> conditions;

    [JsonProperty("triggers")]
    public List<DialogueTriggerDto> triggers;
}

[Serializable]
public class DialogueConditionDto
{
    [JsonProperty("type")]
    public string type;

    [JsonProperty("key")]
    public string key;

    [JsonProperty("stat")]
    public int stat;

    [JsonProperty("op")]
    public string op;

    [JsonProperty("value")]
    public JToken value;
}

[Serializable]
public class DialogueTriggerDto
{
    [JsonProperty("type")]
    public string type;

    [JsonProperty("key")]
    public string key;

    [JsonProperty("value")]
    public JToken value;
}
