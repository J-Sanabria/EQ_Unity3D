using System;
using System.Collections.Generic;
using UnityEngine;

public class RouteShuffleController : MonoBehaviour
{
    [Serializable]
    public class PhaseRouteGroup
    {
        public GameObject metals;
        public GameObject nonMetals;
        public GameObject hydrogen;
        public GameObject oxygen;
    }

    [Serializable]
    public class RouteLayout
    {
        public string id;
        public GameObject root;
        public PhaseRouteGroup phaseRoutes;
    }

    [Header("Layouts completos de rutas")]
    [SerializeField] private List<RouteLayout> layouts = new();

    [Header("Behavior")]
    [SerializeField] private bool avoidImmediateRepeat = true;
    [SerializeField] private bool keepSameLayoutOnRetry = true;

    private readonly Dictionary<int, int> assignedLayoutByReactionIndex = new();
    private readonly List<int> bag = new();

    private int currentLayoutIndex = -1;
    private int lastLayoutIndex = -1;

    public int CurrentLayoutIndex => currentLayoutIndex;

    private void Awake()
    {
        DeactivateAll();
    }

    public void ResetRun()
    {
        assignedLayoutByReactionIndex.Clear();
        bag.Clear();
        currentLayoutIndex = -1;
        lastLayoutIndex = -1;
        DeactivateAll();
    }

    public void ApplyLayoutForReaction(int reactionIndex, HashSet<PhaseKey> presentPhases)
    {
        if (!IsValid())
            return;

        int layoutIndex;

        if (keepSameLayoutOnRetry && assignedLayoutByReactionIndex.TryGetValue(reactionIndex, out layoutIndex))
        {
            SetActiveLayout(layoutIndex, presentPhases);
            return;
        }

        layoutIndex = DrawNextLayoutIndex();
        assignedLayoutByReactionIndex[reactionIndex] = layoutIndex;
        SetActiveLayout(layoutIndex, presentPhases);
    }

    public void ApplySpecificLayout(int layoutIndex, HashSet<PhaseKey> presentPhases)
    {
        if (!IsValid())
            return;

        if (layoutIndex < 0 || layoutIndex >= layouts.Count)
        {
            Debug.LogError($"[RouteShuffleController] layoutIndex fuera de rango: {layoutIndex}");
            return;
        }

        SetActiveLayout(layoutIndex, presentPhases);
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] != null && layouts[i].root != null)
                layouts[i].root.SetActive(false);
        }

        currentLayoutIndex = -1;
    }

    private int DrawNextLayoutIndex()
    {
        RefillBagIfNeeded();

        if (bag.Count == 0)
        {
            Debug.LogError("[RouteShuffleController] No hay layouts disponibles.");
            return -1;
        }

        int pickPosition = UnityEngine.Random.Range(0, bag.Count);
        int chosen = bag[pickPosition];

        if (avoidImmediateRepeat && layouts.Count > 1 && chosen == lastLayoutIndex)
        {
            for (int i = 0; i < bag.Count; i++)
            {
                if (bag[i] != lastLayoutIndex)
                {
                    pickPosition = i;
                    chosen = bag[i];
                    break;
                }
            }
        }

        bag.RemoveAt(pickPosition);
        return chosen;
    }

    private void RefillBagIfNeeded()
    {
        if (bag.Count > 0)
            return;

        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] != null && layouts[i].root != null)
                bag.Add(i);
        }
    }

    private void SetActiveLayout(int layoutIndex, HashSet<PhaseKey> presentPhases)
    {
        if (layoutIndex < 0 || layoutIndex >= layouts.Count)
            return;

        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] == null || layouts[i].root == null)
                continue;

            bool active = (i == layoutIndex);
            layouts[i].root.SetActive(active);

            if (active)
                ApplyPhaseVisibility(layouts[i], presentPhases);
        }

        currentLayoutIndex = layoutIndex;
        lastLayoutIndex = layoutIndex;
    }

    private void ApplyPhaseVisibility(RouteLayout layout, HashSet<PhaseKey> presentPhases)
    {
        if (layout == null || layout.phaseRoutes == null)
            return;

        SetPhaseRoute(layout.phaseRoutes.metals, presentPhases.Contains(PhaseKey.Metals));
        SetPhaseRoute(layout.phaseRoutes.nonMetals, presentPhases.Contains(PhaseKey.NonMetals));
        SetPhaseRoute(layout.phaseRoutes.hydrogen, presentPhases.Contains(PhaseKey.Hydrogen));
        SetPhaseRoute(layout.phaseRoutes.oxygen, presentPhases.Contains(PhaseKey.Oxygen));
    }

    private void SetPhaseRoute(GameObject route, bool active)
    {
        if (route != null)
            route.SetActive(active);
    }

    private bool IsValid()
    {
        if (layouts == null || layouts.Count == 0)
        {
            Debug.LogError("[RouteShuffleController] No hay layouts configurados.");
            return false;
        }

        int validRoots = 0;

        for (int i = 0; i < layouts.Count; i++)
        {
            if (layouts[i] != null && layouts[i].root != null)
                validRoots++;
        }

        if (validRoots == 0)
        {
            Debug.LogError("[RouteShuffleController] Todos los layouts están nulos.");
            return false;
        }

        return true;
    }
}