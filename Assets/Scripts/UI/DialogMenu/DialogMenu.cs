using System;
using TMPro;
using UI;
using UnityEngine;

public class DialogMenu : MonoBehaviour
{
    public bool DialogMenuOpen;
    public bool CanBeOpened;
    public static DialogMenu Singleton;
    public TMP_Text dialogText;
    
    private void HandleUIKeys()
    {
        if (CanBeOpened && Input.GetKeyDown(KeyCode.V) && !ConsoleMenu.Singleton.ConsoleMenuOpen)
        {
            DialogMenuOpen = !DialogMenuOpen;
            Time.timeScale = DialogMenuOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(DialogMenuOpen ? 0 : 9999, DialogMenuOpen ? 0 : 9999, 0);
            StartDialog();
        }
    }

    public void StartDialog()
    {
        DialogueController.Singleton.StartDialogue(DialogueController.Singleton.CurrentNPC);
        dialogText.text = DialogueController.Singleton.GetCurrentStep().Text;
    }
    
    void Update()
    {
        HandleUIKeys();
    }

    public void UpdateText(string text)
    {
        dialogText.text = text;
    }

    public void CloseDialog()
    {
        DialogMenuOpen = false;
        Time.timeScale = DialogMenuOpen ? 0f : 1f;
        this.transform.localPosition = new Vector3(DialogMenuOpen ? 0 : 9999, DialogMenuOpen ? 0 : 9999, 0);
    }

    private void Awake()
    {
        Singleton = this;
    }
}
