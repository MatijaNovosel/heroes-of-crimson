using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UI;

public class TalentTreeContainer : MonoBehaviour
{
    public GameObject talentNodePrefab;
    public GameObject connectorPrefab;

    public RectTransform nodeLayer;
    public RectTransform connectorLayer;

    public float spacingX = 220f;
    public float spacingY = 160f;

    public float paddingLeft = 80f;
    public float paddingTop = 80f;
    public float paddingRight = 80f;
    public float paddingBottom = 80f;

    private readonly Dictionary<int, TalentTreeItem> _itemsByTalentId = new();
    private readonly Dictionary<int, Vector2> _nodePositions = new();

    private void Awake()
    {
        SetupContent();
        EnsureLayers();
    }

    public void Init(List<TalentModel> talents)
    {
        ClearChildren(nodeLayer);
        ClearChildren(connectorLayer);

        _itemsByTalentId.Clear();
        _nodePositions.Clear();

        if (talents == null || talents.Count == 0) return;

        BuildPositions(talents);
        NormalizePositionsAndResizeContent();
        SpawnNodes(talents);

        StartCoroutine(DrawConnectorsNextFrame(talents));
    }

    private void SetupContent()
    {
        var content = (RectTransform)transform;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;
    }

    private void EnsureLayers()
    {
        SetupLayer(nodeLayer);
        SetupLayer(connectorLayer);

        if (connectorLayer != null) connectorLayer.SetAsFirstSibling();
        if (nodeLayer != null) nodeLayer.SetAsLastSibling();
    }

    private void SetupLayer(RectTransform layer)
    {
        if (layer == null) return;
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.offsetMin = Vector2.zero;
        layer.offsetMax = Vector2.zero;
        layer.pivot = new Vector2(0.5f, 0.5f);
    }

    private int GetDepth(
        int talentId,
        Dictionary<int, int> cache,
        HashSet<int> visiting,
        Dictionary<int, TalentModel> byId)
    {
        if (cache.TryGetValue(talentId, out var cached)) return cached;
        if (!byId.TryGetValue(talentId, out var talent)) return 0;
        if (!visiting.Add(talentId)) return 0;

        var parents = GetParentIds(talent).Distinct().Where(byId.ContainsKey).ToList();

        int depth = 0;
        if (parents.Count != 0)
        {
            depth = parents.Max(parentId => GetDepth(parentId, cache, visiting, byId)) + 1;
        }

        visiting.Remove(talentId);
        cache[talentId] = depth;
        return depth;
    }

    private void BuildPositions(List<TalentModel> talents)
    {
        var byId = talents.Where(t => t != null).ToDictionary(t => t.Id, t => t);

        var depthCache = new Dictionary<int, int>();
        var depthBuckets = new Dictionary<int, List<TalentModel>>();

        foreach (var talent in talents.Where(t => t != null))
        {
            int depth = talent.LayoutHint != null
                ? GetDepth(talent.Id, depthCache, new HashSet<int>(), byId)
                : 0;

            if (!depthBuckets.TryGetValue(depth, out var list))
            {
                list = new List<TalentModel>();
                depthBuckets[depth] = list;
            }

            list.Add(talent);
        }

        foreach (var depthPair in depthBuckets.OrderBy(x => x.Key))
        {
            var row = depthPair.Value
                .OrderBy(t => t.LayoutHint != null ? t.LayoutHint.X : int.MaxValue)
                .ThenBy(t => t.Id)
                .ToList();

            for (int i = 0; i < row.Count; i++)
            {
                var talent = row[i];
                int x = talent.LayoutHint != null ? talent.LayoutHint.X : i;
                int y = depthPair.Key;

                _nodePositions[talent.Id] = new Vector2(x * spacingX, -y * spacingY);
            }
        }
    }

    private void NormalizePositionsAndResizeContent()
    {
        if (_nodePositions.Count == 0) return;

        float minX = _nodePositions.Values.Min(v => v.x);
        float maxX = _nodePositions.Values.Max(v => v.x);
        float minY = _nodePositions.Values.Min(v => v.y);
        float maxY = _nodePositions.Values.Max(v => v.y);

        Vector2 shift = new Vector2(
            paddingLeft - minX,
            -paddingTop - maxY
        );

        var keys = _nodePositions.Keys.ToList();
        foreach (var key in keys)
        {
            _nodePositions[key] += shift;
        }

        float width = (maxX - minX) + paddingLeft + paddingRight;
        float height = (maxY - minY) + paddingTop + paddingBottom;

        var content = (RectTransform)transform;
        content.sizeDelta = new Vector2(width, height);
    }

    private void SpawnNodes(List<TalentModel> talents)
    {
        foreach (var talent in talents.Where(t => t != null))
        {
            var go = Instantiate(talentNodePrefab, nodeLayer);
            var rt = go.GetComponent<RectTransform>();
            var item = go.GetComponent<TalentTreeItem>();

            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            item.Init(talent);

            if (_nodePositions.TryGetValue(talent.Id, out var pos))
            {
                rt.anchoredPosition = pos;
            }

            _itemsByTalentId[talent.Id] = item;
        }
    }

    private IEnumerator DrawConnectorsNextFrame(List<TalentModel> talents)
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (nodeLayer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(nodeLayer);
        if (connectorLayer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(connectorLayer);

        ClearChildren(connectorLayer);

        DrawPrerequisiteConnectors(talents);
        DrawExclusiveConnectors(talents);
    }

    private void DrawPrerequisiteConnectors(List<TalentModel> talents)
    {
        foreach (var talent in talents.Where(t => t != null))
        {
            if (!_itemsByTalentId.TryGetValue(talent.Id, out var childItem)) continue;

            foreach (var parentId in GetParentIds(talent))
            {
                if (!_itemsByTalentId.TryGetValue(parentId, out var parentItem)) continue;
                CreateConnector(parentItem.RectTransform, childItem.RectTransform, Color.white);
            }
        }
    }

    private void DrawExclusiveConnectors(List<TalentModel> talents)
    {
        var grouped = talents
            .Where(t => t != null && t.ExclusiveGroupId.HasValue)
            .GroupBy(t => t.ExclusiveGroupId.Value);

        foreach (var group in grouped)
        {
            var groupTalents = group.Where(t => _itemsByTalentId.ContainsKey(t.Id)).ToList();
            if (groupTalents.Count < 2) continue;

            for (int i = 0; i < groupTalents.Count; i++)
            {
                for (int j = i + 1; j < groupTalents.Count; j++)
                {
                    var a = _itemsByTalentId[groupTalents[i].Id].RectTransform;
                    var b = _itemsByTalentId[groupTalents[j].Id].RectTransform;
                    CreateConnector(a, b, Color.red);
                }
            }
        }
    }

    private IEnumerable<int> GetParentIds(TalentModel talent)
    {
        if (talent.RequirementGroups == null) yield break;

        foreach (var group in talent.RequirementGroups)
        {
            if (group?.TalentIds == null) continue;
            foreach (var id in group.TalentIds) yield return id;
        }
    }

    private void CreateConnector(RectTransform from, RectTransform to, Color color)
    {
        if (connectorPrefab == null || connectorLayer == null || from == null || to == null)
        {
            return;
        }

        var go = Instantiate(connectorPrefab, connectorLayer);
        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();

        if (img != null)
        {
            img.raycastTarget = false;
            img.color = color;
        }

        Vector2 start = WorldToLocalPoint(connectorLayer, from.TransformPoint(from.rect.center));
        Vector2 end = WorldToLocalPoint(connectorLayer, to.TransformPoint(to.rect.center));

        Vector2 dir = end - start;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (start + end) * 0.5f;
        rt.sizeDelta = new Vector2(distance, 4f);
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector2 WorldToLocalPoint(RectTransform target, Vector3 worldPosition)
    {
        return target.InverseTransformPoint(worldPosition);
    }

    private void ClearChildren(RectTransform root)
    {
        if (root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}