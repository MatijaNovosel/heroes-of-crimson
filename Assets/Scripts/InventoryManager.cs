using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot
{
  public Item Item { get; set; }
}

public class InventoryManager : MonoBehaviour
{
  public Canvas canvas;
  readonly List<List<InventorySlot>> inventory;
  private readonly int ROWS = 4;
  private readonly int COLS = 4;
  private readonly int invX = Screen.width - 210;
  private readonly int invY = Screen.height - 215;
  private readonly int h = 180;
  private readonly bool showInventory = true;
  private readonly int width = 4;
  private readonly int height = 4;
  private readonly int boxSize = 20;
  private readonly int boxSpacing = 4;

  void DrawRectangle(int x, int y, int width, int height)
  {
    // Change this to something normal lol
    GameObject imageGameObject = new GameObject();
    imageGameObject.transform.SetParent(canvas.transform);

    Vector3 pos = new Vector3(x, y, 0);

    Image image = imageGameObject.AddComponent<Image>();
    image.rectTransform.sizeDelta = new Vector2(width, height);
    image.color = new Color(1.0F, 0.0F, 0.0F);

    imageGameObject.transform.position = pos;
  }

  void Start()
  {
    if (showInventory)
    {
      for (var yy = 0; yy < COLS; yy++)
      {
        for (var xx = 0; xx < ROWS; xx++)
        {
          // X: 362.5, Y: 97.8
          var tx = invX + (xx * (boxSpacing + boxSize));
          var ty = invY + (yy * (boxSpacing + boxSize));
          DrawRectangle(tx, ty, 20, 20);
        }
      }
    }
  }

  void Update()
  {

  }
}
