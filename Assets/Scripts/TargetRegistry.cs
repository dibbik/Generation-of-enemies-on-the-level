using UnityEngine;
using System.Collections.Generic;

public class TargetRegistry : MonoBehaviour
{
    private static TargetRegistry _instance;
    private List<HealthSystem> _allTargets = new List<HealthSystem>();
    private List<TargetFinder> _allFinders = new List<TargetFinder>();

    public static TargetRegistry Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void RegisterTarget(HealthSystem target)
    {
        if (!_allTargets.Contains(target))
        {
            _allTargets.Add(target);
            target.DeathEvent += () => UnregisterTarget(target);

            foreach (var finder in _allFinders)
            {
                finder.RegisterPotentialTarget(target);
            }
        }
    }

    public void RegisterFinder(TargetFinder finder)
    {
        if (!_allFinders.Contains(finder))
        {
            _allFinders.Add(finder);

            foreach (var target in _allTargets)
            {
                finder.RegisterPotentialTarget(target);
            }
        }
    }

    public void UnregisterTarget(HealthSystem target)
    {
        _allTargets.Remove(target);
    }

    public void UnregisterFinder(TargetFinder finder)
    {
        _allFinders.Remove(finder);
    }
}