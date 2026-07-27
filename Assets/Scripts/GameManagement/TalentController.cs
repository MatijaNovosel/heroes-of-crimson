using System.Collections.Generic;
using System.Linq;
using HeroesOfCrimson.Utils;
using Models;
using Newtonsoft.Json;
using UnityEngine;

namespace GameManagement
{
    public class TalentController : MonoBehaviour
    {
        public static TalentController Singleton;

        private const string TalentsResourcesFolder = "Misc/Talents";

        private readonly List<TalentModel> _allTalents = new();
        private readonly Dictionary<Constants.Character, List<TalentModel>> _talentsByClass = new();
        private readonly Dictionary<int, ExclusiveTalentGroupModel> _exclusiveGroupsById = new();

        private void Awake()
        {
            Singleton = this;
            _loadTalents();
        }

        private void _loadTalents()
        {
            _allTalents.Clear();
            _talentsByClass.Clear();
            _exclusiveGroupsById.Clear();

            var files = Resources.LoadAll<TextAsset>(TalentsResourcesFolder);

            if (files == null || files.Length == 0)
            {
                Debug.LogError($"No talent json files found in Resources/{TalentsResourcesFolder}");
                return;
            }

            var file = files.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.text));

            if (file == null)
            {
                Debug.LogError($"No valid talent json content found in Resources/{TalentsResourcesFolder}");
                return;
            }

            TalentFileDto parsed;

            try
            {
                parsed = JsonConvert.DeserializeObject<TalentFileDto>(file.text);
            }
            catch
            {
                Debug.LogError($"Failed to parse talent file: {file.name}");
                return;
            }

            if (parsed == null)
            {
                Debug.LogError($"Talent file '{file.name}' deserialized to null.");
                return;
            }

            if (parsed.exclusiveGroups != null)
            {
                foreach (var groupDto in parsed.exclusiveGroups.Where(x => x != null))
                {
                    var group = ToExclusiveGroupModel(groupDto);
                    if (group != null)
                    {
                        _exclusiveGroupsById[group.Id] = group;
                    }
                }
            }

            if (parsed.talents != null)
            {
                foreach (var talentDto in parsed.talents.Where(x => x != null))
                {
                    var talent = ToTalentModel(talentDto);
                    if (talent == null) continue;

                    _allTalents.Add(talent);

                    if (!_talentsByClass.TryGetValue(talent.Character, out var list))
                    {
                        list = new List<TalentModel>();
                        _talentsByClass[talent.Character] = list;
                    }

                    list.Add(talent);
                }
            }

            Debug.Log($"Loaded {_allTalents.Count} talent(s) across {_talentsByClass.Count} class(es) and {_exclusiveGroupsById.Count} exclusive group(s).");
        }

        private static ExclusiveTalentGroupModel ToExclusiveGroupModel(TalentExclusiveGroupDto dto)
        {
            if (dto == null) return null;

            return new ExclusiveTalentGroupModel
            {
                Id = dto.id,
                Rule = dto.rule
            };
        }

        private static TalentModel ToTalentModel(TalentDto dto)
        {
            if (dto == null) return null;

            var prerequisites = new List<TalentPrerequisiteRuleModel>();

            if (dto.prerequisites != null)
            {
                foreach (var prereq in dto.prerequisites.Where(x => x != null))
                {
                    prerequisites.Add(new TalentPrerequisiteRuleModel
                    {
                        Type = prereq.type,
                        TalentIds = prereq.talents != null ? new List<int>(prereq.talents) : new List<int>()
                    });
                }
            }

            return new TalentModel
            {
                Id = dto.id,
                Name = dto.name,
                Description = dto.description,
                Character = (Constants.Character)dto.character,
                Sprite = ResourceCacher.Singleton.TalentSprites.FirstOrDefault(x => x.name == dto.spritePath),
                SpritePath = dto.spritePath,
                LevelReq = dto.levelReq,
                Tier = dto.tier,
                Row = dto.row,
                ExclusiveGroupId = dto.exclusiveGroupId,
                Prerequisites = prerequisites
            };
        }

        public List<TalentModel> GetTalentsForClass(Constants.Character character)
        {
            return _talentsByClass.TryGetValue(character, out var list)
                ? list.OrderBy(t => t.Tier).ThenBy(t => t.Row).ToList()
                : new List<TalentModel>();
        }

        public List<TalentModel> GetAllTalents()
        {
            return _allTalents
                .OrderBy(t => t.Character)
                .ThenBy(t => t.Tier)
                .ThenBy(t => t.Row)
                .ToList();
        }

        public ExclusiveTalentGroupModel GetExclusiveGroup(int id)
        {
            return _exclusiveGroupsById.TryGetValue(id, out var group) ? group : null;
        }

        public TalentModel GetTalent(int id)
        {
            return _allTalents.FirstOrDefault(t => t.Id == id);
        }

        public bool IsTalentInExclusiveGroup(int talentId, int groupId)
        {
            var talent = GetTalent(talentId);
            return talent != null &&
                   talent.ExclusiveGroupId.HasValue &&
                   talent.ExclusiveGroupId.Value == groupId;
        }
    }
}