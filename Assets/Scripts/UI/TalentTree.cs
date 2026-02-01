using UI;
using UnityEngine;

public class TalentTree : MonoBehaviour
{
    public bool TalentTreeOpen = true;
    
    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.T) && !ConsoleMenu.Singleton.ConsoleMenuOpen)
        {
            TalentTreeOpen = !TalentTreeOpen;
            this.transform.localPosition = new Vector3(TalentTreeOpen ? 0 : 9999, 0, 0);
        }
    }

    public void CloseTalentTree()
    {
        TalentTreeOpen = false;
        this.transform.localPosition = new Vector3(9999, 0, 0);
    }
    
    void Start()
    {
        //
    }

    void Update()
    {
        HandleUIKeys();
    }
}
