using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public class ExclusiveTalentGroupModel
    {
        public int Id;
        public string Rule;
    } 
    
    public class TalentPrerequisiteRuleModel
    {
        public string Type;
        public List<int> TalentIds;
    }
    
    public class TalentModel
    {
        public int Id;
        public string Name;
        public string Description;
        public Constants.Character Character;
        public Sprite Sprite;
        public string SpritePath;
        public int LevelReq;
        public int Tier;
        public int Row;
        public int? ExclusiveGroupId;
        public List<TalentPrerequisiteRuleModel> Prerequisites;
    }
}