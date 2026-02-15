using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionItem : MonoBehaviour
{
    public TMP_Text textField;
    public Button button;
    public string choiceId;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Init(string value, string id)
    {
        this.textField.text = value;
        this.choiceId = id;
        button.onClick.AddListener(() =>
        {
            DialogueController.Singleton.ChooseOption(choiceId);
        });
    }
}
