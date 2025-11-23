using UnityEngine;

public class DialogMenu : MonoBehaviour
{
    public bool DialogMenuOpen;
    
    private void HandleUIKeys()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            DialogMenuOpen = !DialogMenuOpen;
            Time.timeScale = DialogMenuOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(DialogMenuOpen ? 0 : 1500, DialogMenuOpen ? 0 : -750, 0);
        }
    }

    void Update()
    {
        HandleUIKeys();
    }
}
