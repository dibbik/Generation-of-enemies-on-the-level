using UnityEngine;
using System.Collections.Generic;

public class TargetFinder : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private bool _activeSearch = true;

    private const float TargetUpdateInterval = 0.3f;
    private const float CacheDuration = 0.5f;

    private float _lastTargetUpdateTime;
    private float _lastCacheTime;
    private Transform _cachedTarget;
    private List<HealthSystem> _potentialTargets = new List<HealthSystem>();

    private void Awake()
    {
        // Автоматически регистрируемся в реестре
        TargetRegistry.Instance?.RegisterFinder(this);
    }

    private void OnDestroy()
    {
        TargetRegistry.Instance?.UnregisterFinder(this);
    }

    public void RegisterPotentialTarget(HealthSystem target)
    {
        if (!_potentialTargets.Contains(target))
        {
            _potentialTargets.Add(target);
            target.DeathEvent += () => UnregisterPotentialTarget(target);
        }
    }

    public void UnregisterPotentialTarget(HealthSystem target)
    {
        _potentialTargets.Remove(target);

        if (_cachedTarget != null && _cachedTarget.GetComponent<HealthSystem>() == target)
        {
            _cachedTarget = null;
        }
    }

    public Transform FindTarget(System.Type targetType)
    {
        if (Time.time - _lastCacheTime < CacheDuration &&
            _cachedTarget != null &&
            IsTargetValid(_cachedTarget, targetType))
        {
            return _cachedTarget;
        }

        if (Time.time - _lastTargetUpdateTime >= TargetUpdateInterval)
        {
            _lastTargetUpdateTime = Time.time;

            Transform newTarget = _activeSearch ?
                FindNearestTarget(targetType) :
                FindTargetInRange(targetType);

            if (newTarget != null)
            {
                _cachedTarget = newTarget;
                _lastCacheTime = Time.time;
            }
            else
            {
                _cachedTarget = null;
            }
        }

        return _cachedTarget;
    }

    private Transform FindNearestTarget(System.Type targetType)
    {
        Transform nearest = null;
        float minSqrDistance = float.MaxValue;
        Vector3 myPosition = transform.position;

        foreach (var target in _potentialTargets)
        {
            if (IsValidTarget(target, targetType))
            {
                float sqrDistance = (target.transform.position - myPosition).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = target.transform;
                }
            }
        }

        return nearest;
    }

    private Transform FindTargetInRange(System.Type targetType)
    {
        Transform closestInRange = null;
        float closestSqrDistance = float.MaxValue;
        float sqrDetectionRange = _detectionRange * _detectionRange;
        Vector3 myPosition = transform.position;

        foreach (var target in _potentialTargets)
        {
            if (IsValidTarget(target, targetType))
            {
                float sqrDistance = (target.transform.position - myPosition).sqrMagnitude;
                if (sqrDistance <= sqrDetectionRange && sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestInRange = target.transform;
                }
            }
        }

        return closestInRange;
    }

    private bool IsValidTarget(HealthSystem target, System.Type targetType)
    {
        return target != null &&
               target.IsAlive &&
               target.GetComponent(targetType) != null;
    }

    private bool IsTargetValid(Transform target, System.Type targetType)
    {
        if (target == null) return false;

        return target.TryGetComponent(out HealthSystem health) &&
               health.IsAlive &&
               target.GetComponent(targetType) != null;
    }

    public void ClearCache()
    {
        _cachedTarget = null;
        _lastCacheTime = 0f;
    }

    public Transform GetCachedTarget()
    {
        return _cachedTarget;
    }
}