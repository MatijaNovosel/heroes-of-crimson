using UI;
using UnityEngine;

public class TalentTree : MonoBehaviour
{
    public bool TalentTreeOpen;
    public static TalentTree Singleton;
    
    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.T) && !ConsoleMenu.Singleton.ConsoleMenuOpen)
        {
            TalentTreeOpen = !TalentTreeOpen;
            Time.timeScale = TalentTreeOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(TalentTreeOpen ? 0 : 9999, 0, 0);
        }
    }

    public void CloseTalentTree()
    {
        TalentTreeOpen = false;
        Time.timeScale = 1f;
        this.transform.localPosition = new Vector3(9999, 0, 0);
    }
    
    void Start()
    {
        Singleton = this;
    }

    void Update()
    {
        HandleUIKeys();
    }
}
