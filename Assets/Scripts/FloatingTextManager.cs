using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
  public GameObject textContainer;
  public GameObject textPrefab;

  private List<FloatingText> floatingTexts = new List<FloatingText>();

  private FloatingText GetFloatingText()
  {
    FloatingText ft = floatingTexts.Find(txt => !txt.active);

    if (ft == null)
    {
      ft = new FloatingText();
      ft.obj = Instantiate(textPrefab);
      ft.obj.transform.SetParent(textContainer.transform);
      ft.text = ft.obj.GetComponent<Text>();
      floatingTexts.Add(ft);
    }

    return ft;
  }

  public void Show(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
  {
    FloatingText ft = GetFloatingText();
    ft.text.text = msg;
    ft.text.fontSize = fontSize;
    ft.text.color = color;
    ft.obj.transform.position = Camera.main.WorldToScreenPoint(position);
    ft.motion = motion;
    ft.duration = duration;
    ft.Show();
  }

  private void Update()
  {
    foreach (FloatingText ft in floatingTexts)
    {
      ft.UpdateFloatingText();
    }
  }
}
