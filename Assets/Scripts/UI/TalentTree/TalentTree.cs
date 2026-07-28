using System.Collections.Generic;
using Models;
using UI;
using UnityEngine;

public class TalentTree : MonoBehaviour
{
    public bool TalentTreeOpen;
    public static TalentTree Singleton;
    public TalentTreeContainer talentTreeContainer;

    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.T) && !ConsoleMenu.Singleton.ConsoleMenuOpen)
        {
            TalentTreeOpen = !TalentTreeOpen;
            Time.timeScale = TalentTreeOpen ? 0f : 1f;
            transform.localPosition = new Vector3(TalentTreeOpen ? 0 : 9999, 0, 0);
        }
    }

    public void CloseTalentTree()
    {
        TalentTreeOpen = false;
        Time.timeScale = 1f;
        transform.localPosition = new Vector3(9999, 0, 0);
    }

    private void Start()
    {
        Singleton = this;
    }

    private void Update()
    {
        HandleUIKeys();
    }

    public void Init(List<TalentModel> talents)
    {
        talentTreeContainer.Init(talents);
    }
}