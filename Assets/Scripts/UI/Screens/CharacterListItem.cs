using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterListItem : MonoBehaviour, IPointerClickHandler
{
    public Constants.Character character;
    public NewGame newGameMenu;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        newGameMenu.SelectCharacter((int)character);
    }
}
