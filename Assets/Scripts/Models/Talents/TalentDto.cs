using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    [JsonProperty("tier")]
    public int tier;

    [JsonProperty("row")]
    public int row;

    [JsonProperty("exclusiveGroupId")]
    public int? exclusiveGroupId;

    [JsonProperty("prerequisites")]
    public List<TalentPrerequisiteDto> prerequisites;
}

[Serializable]
public class TalentPrerequisiteDto
{
    [JsonProperty("type")]
    public string type;

    [JsonProperty("talents")]
    public List<int> talents;
}

[Serializable]
public class TalentExclusiveGroupDto
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("rule")]
    public string rule;
}