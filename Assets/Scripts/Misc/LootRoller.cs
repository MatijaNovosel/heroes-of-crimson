using System;
using System.Collections.Generic;
using UnityEngine;
using Models;

public static class LootRoller
{
    private static int PickWeighted(List<ItemDropModel> drops)
    {
        float total = 0f;
        for (int i = 0; i < drops.Count; i++) total += Mathf.Max(0f, drops[i].DropChance);
        if (total <= 0f) return drops[UnityEngine.Random.Range(0, drops.Count)].ItemId;

        float roll = UnityEngine.Random.Range(0f, total);
        float running = 0f;

        for (int i = 0; i < drops.Count; i++)
        {
            running += Mathf.Max(0f, drops[i].DropChance);
            if (roll < running) return drops[i].ItemId;
        }

        return drops[drops.Count - 1].ItemId;
    }

    public static int[] RollGuaranteed(LootTableModel table, int itemCount, int? seed = null)
    {
        if (table.Items == null || table.Items.Count == 0)
        {
            return Array.Empty<int>();
        }

        itemCount = Mathf.Max(1, itemCount);

        if (seed.HasValue)
        {
            UnityEngine.Random.InitState(seed.Value);
        }

        var result = new List<int>(itemCount);
        var guaranteed = new List<ItemDropModel>();
        var nonGuaranteed = new List<ItemDropModel>();

        for (int i = 0; i < table.Items.Count; i++)
        {
            var d = table.Items[i];
            if (d.Guaranteed == true) guaranteed.Add(d);
            else nonGuaranteed.Add(d);
        }

        if (guaranteed.Count > 0)
        {
            if (guaranteed.Count >= itemCount)
            {
                for (int i = 0; i < itemCount; i++)
                {
                    int idx = UnityEngine.Random.Range(0, guaranteed.Count);
                    result.Add(guaranteed[idx].ItemId);
                }
                return result.ToArray();
            }

            for (int i = 0; i < guaranteed.Count; i++)
            {
                result.Add(guaranteed[i].ItemId);
            }
        }

        var rollPool = (nonGuaranteed.Count > 0) ? nonGuaranteed : table.Items;
        while (result.Count < itemCount) result.Add(PickWeighted(rollPool));

        return result.ToArray();
    }
}