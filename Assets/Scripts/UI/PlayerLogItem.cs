using TMPro;
using UnityEngine;

public class PlayerLogItem : MonoBehaviour
{
    private TMP_Text _text;
    private string _value;
    private int _id;
    
    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        
    }

    public int GetId()
    {
        return _id;
    }

    public void Initialize(string value, int id)
    {
        _value = value;
        _id = id;
        _text.text = _value;
    }
}
