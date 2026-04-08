using System;
using HeroesOfCrimson.Utils;
using UnityEngine;

public class CentralWindowTabs: MonoBehaviour
{
    public static CentralWindowTabs Singleton;
    public Constants.CentralWindowTabsEnum ActiveTab;

    public void SetActiveTab(int tab)
    {
        ActiveTab = (Constants.CentralWindowTabsEnum)tab;
    }
    
    private void Awake()
    {
        Singleton = this;
    }
}