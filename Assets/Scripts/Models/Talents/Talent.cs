using System.Collections.Generic;
using HeroesOfCrimson.Utils;
using UnityEngine;

namespace Models
{
    public enum TalentRequirementMode
    {
        AllOf,
        AnyOf
    }

    public class TalentRequirementGroupModel
    {
        public TalentRequirementMode Mode;
        public List<int> TalentIds = new();
    }

    public class TalentLayoutHintModel
    {
        public int X;
        public int Y;
    }

    public class ExclusiveTalentGroupModel
    {
        public int Id;
        public string Rule;
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
        public int? ExclusiveGroupId;

        public List<TalentRequirementGroupModel> RequirementGroups = new();
        public TalentLayoutHintModel LayoutHint;
    }
}