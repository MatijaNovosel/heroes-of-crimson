using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public GameObject textPrefab;

    [SerializeField] private int prewarmCount = 12;

    private readonly List<FloatingText> floatingTexts = new List<FloatingText>();

    private void Awake()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            CreateFloatingText();
        }
    }

    private FloatingText CreateFloatingText()
    {
        var obj = Instantiate(textPrefab);
        obj.SetActive(false);

        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.sortingLayerName = "UI";
        renderer.sortingOrder = 50;

        var ft = new FloatingText
        {
            obj = obj,
            text = obj.GetComponent<TMP_Text>(),
            active = false
        };

        floatingTexts.Add(ft);
        return ft;
    }

    private FloatingText GetFloatingText()
    {
        var ft = floatingTexts.Find(txt => !txt.active);
        return ft ?? CreateFloatingText();
    }

    public FloatingText Show(
        string msg,
        int fontSize,
        Color color,
        Vector3 position,
        Vector3 motion,
        float duration
    )
    {
        position.y += 0.05f;

        var ft = GetFloatingText();

        ft.text.text = msg;
        ft.text.fontSize = 10 * (fontSize / 100f);
        ft.text.color = color;
        ft.text.fontStyle = TMPro.FontStyles.Bold;

        ft.obj.transform.position = position;
        ft.motion = motion;
        ft.duration = duration;
        ft.Show();

        return ft;
    }

    private void Update()
    {
        floatingTexts.ForEach(ft => ft.UpdateFloatingText());
    }
}