using System;
using HeroesOfCrimson.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGame : MonoBehaviour
{
    public int selectedCharacter = (int)Constants.Character.Mage;
    public Image knightCharacterImage;
    public Image mageCharacterImage;
    public Image rangerCharacterImage;

    public void Start()
    {
        SelectCharacter((int)Constants.Character.Mage);
    }

    public void Continue()
    {
        SceneManager.LoadScene(2);
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }

    public void SelectCharacter(int character)
    {
        selectedCharacter = character;
        switch (selectedCharacter)
        {
            case (int)Constants.Character.Knight:
            {
                knightCharacterImage.color = Color.seaGreen;
                mageCharacterImage.color = Color.black;
                rangerCharacterImage.color = Color.black;
                break;
            }
            case (int)Constants.Character.Mage:
            {
                mageCharacterImage.color = Color.seaGreen;
                knightCharacterImage.color = Color.black;
                rangerCharacterImage.color = Color.black;
                break;
            }
            case (int)Constants.Character.Ranger:
            {
                rangerCharacterImage.color = Color.seaGreen;
                mageCharacterImage.color = Color.black;
                knightCharacterImage.color = Color.black;
                break;
            }
        }
    }
}
