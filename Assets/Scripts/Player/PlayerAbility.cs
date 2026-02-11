using System.Collections.Generic;
using GameManagement;
using HeroesOfCrimson.Utils;
using JetBrains.Annotations;
using Models;
using Models.Player;
using UI;
using UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerAbility : MonoBehaviour
{
    private Player _player;
    
    [Header("Abilities")]
    public AbilitySlot[] abilities = new AbilitySlot[4];
    public List<Tilemap> forbiddenAbilityTilemaps;
    
    private void HandleAbilityCooldowns()
    {
        foreach (var ability in abilities)
        {
            if (ability.cooldownImage is null) continue;
            if (ability.IsReady) ability.cooldownImage.fillAmount = 1f;
            else ability.cooldownImage.fillAmount = ability.CooldownRemaining / ability.cooldown;
        }
    }
    
    private bool _isCursorOverForbiddenAbilityTile()
    {
        Vector3 mouseWorldPos = Utils.GetMousePosition();

        foreach (var tilemap in forbiddenAbilityTilemaps)
        {
            if (!tilemap) continue;
            Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);
            if (tilemap.HasTile(cellPos)) return true;
        }

        return false;
    }
    
    private GameObject _getAbilityPrefab(Constants.AbilityType abilityType)
    {
        return abilityType switch
        {
            Constants.AbilityType.Meteor => Resources.Load<GameObject>("Prefabs/Abilities/Meteor"),
            Constants.AbilityType.FireSphere => Resources.Load<GameObject>("Prefabs/Abilities/FireSphere"),
            _ => null
        };
    }
    
    private void HandleAbility()
    {
        if (_player.HasStatusEffect(Constants.StatusEffects.Silenced) || ConsoleMenu.Singleton.ConsoleMenuOpen) return;
    
        HandleAbilityCooldowns();

        foreach (var ability in abilities)
        {
            if (!Input.GetKeyDown(ability.key) || !ability.IsReady) continue;

            if (_isCursorOverForbiddenAbilityTile() || _player.CurrentMana < ability.manaCost)
            {
                AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Error);
                continue;
            }

            var prefab = _getAbilityPrefab(ability.abilityType);

            _player.CurrentMana -= ability.manaCost;
            ability.lastUsedTime = Time.time;

            var cursorPosition = Utils.GetMousePosition();

            switch (ability.abilityType)
            {
                case Constants.AbilityType.Meteor:
                case Constants.AbilityType.FireSphere:
                    var abilityObj = Instantiate(
                        prefab,
                        new Vector3(cursorPosition.x, cursorPosition.y + 0.8f, 0),
                        Quaternion.identity
                    );

                    if (abilityObj.TryGetComponent<Meteor>(out var meteor))
                    {
                        meteor.Init(cursorPosition);
                    }
                    else if (abilityObj.TryGetComponent<FireSphere>(out var fireSphere))
                    {
                        fireSphere.Init();
                    }
          
                    break;
                case Constants.AbilityType.Teleport:
                    transform.position = cursorPosition;
                    AudioManager.Singleton.PlaySoundCached(Constants.Sounds.Teleport);
                    ParticleManager.Singleton.SpawnParticles(transform, Color.white, 50);
                    break;
            }
        }
    }
    
    void Start()
    {
        _player = GetComponent<Player>();
    }

    void Update()
    {
        HandleAbility();
    }
}
