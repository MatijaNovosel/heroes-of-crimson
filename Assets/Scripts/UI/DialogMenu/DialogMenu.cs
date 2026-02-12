using UI;
using UnityEngine;

public class DialogMenu : MonoBehaviour
{
    public bool DialogMenuOpen;
    
    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.V) && !ConsoleMenu.Singleton.ConsoleMenuOpen)
        {
            DialogMenuOpen = !DialogMenuOpen;
            Time.timeScale = DialogMenuOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(DialogMenuOpen ? 0 : 9999, DialogMenuOpen ? 0 : 9999, 0);
        }
    }

    void Update()
    {
        HandleUIKeys();
    }
}
