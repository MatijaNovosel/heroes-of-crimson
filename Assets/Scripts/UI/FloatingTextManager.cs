using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
  public GameObject textPrefab;

  private List<FloatingText> floatingTexts = new List<FloatingText>();

  private FloatingText GetFloatingText()
  {
    var ft = floatingTexts.Find(txt => !txt.active);

    if (ft == null)
    {
      ft = new FloatingText
      {
        obj = Instantiate(textPrefab)
      };
      ft.obj.GetComponent<MeshRenderer>().sortingLayerName = "Collision";
      ft.obj.GetComponent<MeshRenderer>().sortingOrder = 50;
      ft.text = ft.obj.GetComponent<TextMesh>();
      floatingTexts.Add(ft);
    }

    return ft;
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
    ft.text.fontSize = fontSize;
    ft.text.color = color;
    ft.text.fontStyle = FontStyle.Bold;
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
