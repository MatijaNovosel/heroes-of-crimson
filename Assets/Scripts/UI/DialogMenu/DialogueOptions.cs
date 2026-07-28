using System.Collections.Generic;
using Models.Dialogue;
using UnityEngine;

public class DialogueOptions : MonoBehaviour
{
    public GameObject dialogueOptionPrefab;
    private Dictionary<string, DialogueOptionItem> _items = new();

    private void _clearItems()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        _items.Clear();
    }
    
    public void Init(List<DialogueChoiceModel> choices)
    {
        _clearItems();

        foreach (var choice in choices)
        {
            var go = Instantiate(dialogueOptionPrefab, transform);
            var item = go.GetComponent<DialogueOptionItem>();
            item.Init(choice.Text, choice.Id);
            _items[choice.Id] = item;
        }
    }
}
