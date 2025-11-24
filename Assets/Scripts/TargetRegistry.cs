using UnityEngine;
using System.Collections.Generic;

public class TargetRegistry : MonoBehaviour
{
    private List<HealthSystem> _allTargets = new List<HealthSystem>();
    private List<TargetFinder> _allFinders = new List<TargetFinder>();
    private Dictionary<HealthSystem, System.Action> _targetDeathHandlers = new Dictionary<HealthSystem, System.Action>();

    public void RegisterTarget(HealthSystem target)
    {
        if (!_allTargets.Contains(target))
        {
            _allTargets.Add(target);

            void DeathHandler() => UnregisterTarget(target);
            target.DeathEvent += DeathHandler;

            _targetDeathHandlers[target] = DeathHandler;

            foreach (var finder in _allFinders)
            {
                finder.RegisterPotentialTarget(target);
            }
        }
    }

    public void UnregisterTarget(HealthSystem target)
    {
        if (_allTargets.Remove(target))
        {
            if (_targetDeathHandlers.TryGetValue(target, out var handler))
            {
                target.DeathEvent -= handler;
                _targetDeathHandlers.Remove(target);
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

    public void UnregisterFinder(TargetFinder finder)
    {
        _allFinders.Remove(finder);
    }

    private void OnDestroy()
    {
        foreach (var kvp in _targetDeathHandlers)
        {
            kvp.Key.DeathEvent -= kvp.Value;
        }
        _targetDeathHandlers.Clear();
    }
}