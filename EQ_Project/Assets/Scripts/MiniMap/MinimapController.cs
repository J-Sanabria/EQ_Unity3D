using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform iconsRoot;
    [SerializeField] private RectTransform playerBlip;

    [Header("Prefabs")]
    [SerializeField] private Image keyBlipPrefab;

    [Header("World bounds (XZ)")]
    [SerializeField] private Transform worldMin;
    [SerializeField] private Transform worldMax;

    [Header("Refs")]
    [SerializeField] private Transform player;

    [Header("Behavior")]
    [SerializeField] private bool rotateMapWithPlayer = true;
    [SerializeField] private float visibleRadiusWorld = 25f;
    [SerializeField] private float updateInterval = 0.1f;

    private readonly List<MinimapTarget> _targets = new();
    private readonly Dictionary<MinimapTarget, Image> _icons = new();

    private float _timer;

    private void Start()
    {
        RebuildTargets();
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;

        if (!HasValidRefs())
            return;

        UpdatePlayer();
        UpdateTargets();
    }

    private bool HasValidRefs()
    {
        return player != null &&
               mapRect != null &&
               iconsRoot != null &&
               worldMin != null &&
               worldMax != null;
    }

    public void RebuildTargets()
    {
        ClearIcons();
        _targets.Clear();

        var found = FindObjectsByType<MinimapTarget>(FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            var target = found[i];
            if (target == null) continue;
            if (!target.IsAvailable()) continue;

            _targets.Add(target);

            if (target.type == MinimapTarget.TargetType.Key && keyBlipPrefab != null)
            {
                Image icon = Instantiate(keyBlipPrefab, iconsRoot);
                icon.gameObject.SetActive(true);
                _icons[target] = icon;
            }
        }
    }

    private void ClearIcons()
    {
        foreach (var kv in _icons)
        {
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        }

        _icons.Clear();

        if (iconsRoot == null) return;

        for (int i = iconsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(iconsRoot.GetChild(i).gameObject);
        }
    }

    private void UpdatePlayer()
    {
        if (playerBlip != null)
            playerBlip.anchoredPosition = Vector2.zero;

        if (rotateMapWithPlayer)
        {
            float yaw = player.eulerAngles.y;
            mapRect.localRotation = Quaternion.Euler(0f, 0f, yaw);
            iconsRoot.localRotation = Quaternion.Euler(0f, 0f, yaw);
        }
        else
        {
            mapRect.localRotation = Quaternion.identity;
            iconsRoot.localRotation = Quaternion.identity;
        }
    }

    private void UpdateTargets()
    {
        Vector2 min = new(worldMin.position.x, worldMin.position.z);
        Vector2 max = new(worldMax.position.x, worldMax.position.z);
        Vector2 playerXZ = new(player.position.x, player.position.z);
        Vector2 mapSize = mapRect.rect.size;

        float pNx = Mathf.InverseLerp(min.x, max.x, playerXZ.x);
        float pNy = Mathf.InverseLerp(min.y, max.y, playerXZ.y);
        float pX = (pNx - 0.5f) * mapSize.x;
        float pY = (pNy - 0.5f) * mapSize.y;

        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            MinimapTarget target = _targets[i];

            if (target == null || !target.IsAvailable())
            {
                RemoveTarget(target);
                continue;
            }

            if (!_icons.TryGetValue(target, out Image icon) || icon == null)
                continue;

            Vector2 txz = new(target.transform.position.x, target.transform.position.z);
            float dist = Vector2.Distance(playerXZ, txz);
            bool visible = dist <= visibleRadiusWorld;

            icon.gameObject.SetActive(visible);
            if (!visible) continue;

            float nx = Mathf.InverseLerp(min.x, max.x, txz.x);
            float ny = Mathf.InverseLerp(min.y, max.y, txz.y);

            float px = (nx - 0.5f) * mapSize.x;
            float py = (ny - 0.5f) * mapSize.y;

            Vector2 rel = new(px - pX, py - pY);
            rel = ClampToRect(rel, mapRect.rect);

            icon.rectTransform.anchoredPosition = rel;

            float a = Mathf.InverseLerp(visibleRadiusWorld, 0f, dist);
            Color c = icon.color;
            c.a = Mathf.Clamp01(0.35f + 0.65f * a);
            icon.color = c;

            float s = Mathf.Lerp(0.7f, 1.15f, a);
            icon.rectTransform.localScale = new Vector3(s, s, 1f);
        }
    }

    private void RemoveTarget(MinimapTarget target)
    {
        if (target != null)
            _targets.Remove(target);
        else
            _targets.RemoveAll(t => t == null);

        if (target != null && _icons.TryGetValue(target, out Image icon))
        {
            if (icon != null)
                Destroy(icon.gameObject);

            _icons.Remove(target);
        }
    }

    private static Vector2 ClampToRect(Vector2 p, Rect r)
    {
        float halfW = r.width * 0.5f;
        float halfH = r.height * 0.5f;

        p.x = Mathf.Clamp(p.x, -halfW, halfW);
        p.y = Mathf.Clamp(p.y, -halfH, halfH);

        return p;
    }
}