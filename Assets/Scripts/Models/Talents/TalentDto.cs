using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class TalentFileDto
{
    [JsonProperty("talents")]
    public List<TalentDto> talents;

    [JsonProperty("exclusiveGroups")]
    public List<TalentExclusiveGroupDto> exclusiveGroups;
}

[Serializable]
public class TalentDto
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("name")]
    public string name;

    [JsonProperty("description")]
    public string description;

    [JsonProperty("character")]
    public int character;

    [JsonProperty("spritePath")]
    public string spritePath;

    [JsonProperty("levelReq")]
    public int levelReq;

    [JsonProperty("exclusiveGroupId")]
    public int? exclusiveGroupId;

    [JsonProperty("requirementGroups")]
    public List<TalentRequirementGroupDto> requirementGroups;

    [JsonProperty("layoutHint")]
    public TalentLayoutHintDto layoutHint;
}

[Serializable]
public class TalentRequirementGroupDto
{
    [JsonProperty("mode")]
    public string mode;

    [JsonProperty("talents")]
    public List<int> talents;
}

[Serializable]
public class TalentLayoutHintDto
{
    [JsonProperty("x")]
    public int x;

    [JsonProperty("y")]
    public int y;
}

[Serializable]
public class TalentExclusiveGroupDto
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("rule")]
    public string rule;
}