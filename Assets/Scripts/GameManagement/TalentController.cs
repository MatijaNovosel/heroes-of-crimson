using System;
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
        public TalentTree talentTree;

        private const string TalentsResourcesFolder = "Misc/Talents";

        private readonly List<TalentModel> _allTalents = new();
        private readonly Dictionary<Constants.Character, List<TalentModel>> _talentsByClass = new();
        private readonly Dictionary<int, ExclusiveTalentGroupModel> _exclusiveGroupsById = new();

        private void Awake()
        {
            Singleton = this;
            _loadTalents();
            RefreshTalentTreeForSelectedCharacter();
        }

        public void RefreshTalentTreeForSelectedCharacter()
        {
            if (talentTree == null) return;
            if (GameManager.Singleton == null) return;

            var selectedCharacter = (Constants.Character)GameManager.Singleton.GetSelectedCharacter();
            var talents = GetTalentsForClass(selectedCharacter);

            talentTree.Init(talents);
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

        private static TalentRequirementMode ParseRequirementMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return TalentRequirementMode.AllOf;

            switch (mode.Trim().ToLowerInvariant())
            {
                case "any":
                case "anyof":
                    return TalentRequirementMode.AnyOf;

                case "all":
                case "allof":
                default:
                    return TalentRequirementMode.AllOf;
            }
        }

        private static TalentModel ToTalentModel(TalentDto dto)
        {
            if (dto == null) return null;

            var requirementGroups = new List<TalentRequirementGroupModel>();

            if (dto.requirementGroups != null)
            {
                foreach (var groupDto in dto.requirementGroups.Where(x => x != null))
                {
                    requirementGroups.Add(new TalentRequirementGroupModel
                    {
                        Mode = ParseRequirementMode(groupDto.mode),
                        TalentIds = groupDto.talents != null ? new List<int>(groupDto.talents) : new List<int>()
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
                ExclusiveGroupId = dto.exclusiveGroupId,
                RequirementGroups = requirementGroups,
                LayoutHint = dto.layoutHint == null ? null : new TalentLayoutHintModel
                {
                    X = dto.layoutHint.x,
                    Y = dto.layoutHint.y
                }
            };
        }

        public List<TalentModel> GetTalentsForClass(Constants.Character character)
        {
            return _talentsByClass.TryGetValue(character, out var list)
                ? list.OrderBy(t => t.LayoutHint != null ? t.LayoutHint.Y : int.MaxValue)
                      .ThenBy(t => t.LayoutHint != null ? t.LayoutHint.X : int.MaxValue)
                      .ThenBy(t => t.Id)
                      .ToList()
                : new List<TalentModel>();
        }

        public List<TalentModel> GetAllTalents()
        {
            return _allTalents
                .OrderBy(t => t.Character)
                .ThenBy(t => t.LayoutHint != null ? t.LayoutHint.Y : int.MaxValue)
                .ThenBy(t => t.LayoutHint != null ? t.LayoutHint.X : int.MaxValue)
                .ThenBy(t => t.Id)
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