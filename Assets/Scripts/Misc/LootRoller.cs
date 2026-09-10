using System;
using System.Collections.Generic;
using UnityEngine;
using Models;

public static class LootRoller
{
    private static int PickWeightedIndex(List<ItemDropModel> drops)
    {
        float total = 0f;

        for (int i = 0; i < drops.Count; i++)
        {
            total += Mathf.Max(0f, drops[i].DropChance);
        }

        // If all weights are zero, just choose one randomly.
        if (total <= 0f)
        {
            return UnityEngine.Random.Range(0, drops.Count);
        }

        float roll = UnityEngine.Random.Range(0f, total);
        float running = 0f;

        for (int i = 0; i < drops.Count; i++)
        {
            running += Mathf.Max(0f, drops[i].DropChance);

            if (roll < running)
            {
                return i;
            }
        }

        return drops.Count - 1;
    }

    public static int[] Roll(
        LootTableModel table,
        int randomItemCount = 0,
        int? seed = null)
    {
        if (table.Items == null || table.Items.Count == 0)
        {
            return Array.Empty<int>();
        }

        randomItemCount = Mathf.Max(0, randomItemCount);

        if (seed.HasValue)
        {
            UnityEngine.Random.InitState(seed.Value);
        }

        var result = new List<int>();
        var randomPool = new List<ItemDropModel>();

        // Guaranteed items always drop exactly once.
        for (int i = 0; i < table.Items.Count; i++)
        {
            ItemDropModel drop = table.Items[i];

            if (drop.Guaranteed)
            {
                result.Add(drop.ItemId);
            }
            else
            {
                randomPool.Add(drop);
            }
        }

        // Roll extra random items without replacement.
        int rolls = Mathf.Min(randomItemCount, randomPool.Count);

        for (int i = 0; i < rolls; i++)
        {
            int index = PickWeightedIndex(randomPool);
            result.Add(randomPool[index].ItemId);
            // Remove it so it cannot be selected again.
            randomPool.RemoveAt(index);
        }

        return result.ToArray();
    }
}