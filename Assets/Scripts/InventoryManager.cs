using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
  public Item item { get; set; }
}

public class InventoryManager : MonoBehaviour
{
  List<List<InventorySlot>> inventory;
  private int ROWS = 4;
  private int COLS = 4;
  private int invX = 598;
  private int invY = 18;
  private int h = 180;
  private bool showInventory = true;
  private int width = 4;
  private int height = 8;
  private int boxSize = 40;
  private int boxSpacing = 8;

  void drawRectangle(int tx, int ty, int width, int height) {
    //
  }

  void Start()
  {
    if (showInventory)
    {
      for (var yy = 0; yy < h; yy++)
      {
        for (var xx = 0; xx < width; xx++)
        {
          var tx = invX + (xx * (boxSpacing + boxSize));
          var ty = invY + (yy * (boxSpacing + boxSize));
          drawRectangle(tx, ty, tx + boxSize, ty + boxSize);
        }
      }
    }
  }

  void Update()
  {

  }
}
