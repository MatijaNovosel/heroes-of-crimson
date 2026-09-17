using HeroesOfCrimson.Utils;
using UnityEngine;
using Constants = HeroesOfCrimson.Utils.Constants;
using Image = UnityEngine.UI.Image;

public class CentralWindowTabBtn : MonoBehaviour
{
    public Constants.CentralWindowTabsEnum Tab;
    private Image _btnImage; 
    
    void Start()
    {
        _btnImage = GetComponent<Image>();
    }

    void Update()
    {
        _btnImage.color = CentralWindowTabs.Singleton.ActiveTab == Tab ? Utils.FromHex("4d0f1d") : Utils.FromHex("1a0f18");
    }
}
