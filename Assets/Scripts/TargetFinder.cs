using UnityEngine;

public class TargetFinder : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private bool _activeSearch = true;

    public Transform FindTarget(System.Type targetType)
    {
        if (_activeSearch)
        {
            return FindNearestTarget(targetType);
        }
        else
        {
            return FindTargetInRange(targetType);
        }
    }

    private Transform FindNearestTarget(System.Type targetType)
    {
        MonoBehaviour[] targets = FindObjectsOfType(targetType) as MonoBehaviour[];
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (MonoBehaviour target in targets)
        {
            if (target != null && IsTargetValid(target.transform))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = target.transform;
                }
            }
        }

        return nearest;
    }

    private Transform FindTargetInRange(System.Type targetType)
    {
        MonoBehaviour[] targets = FindObjectsOfType(targetType) as MonoBehaviour[];
        Transform closestInRange = null;
        float closestDistance = float.MaxValue;

        foreach (MonoBehaviour target in targets)
        {
            if (target != null && IsTargetValid(target.transform))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance <= _detectionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInRange = target.transform;
                }
            }
        }

        return closestInRange;
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) 
            return false;

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        return targetHealth != null && targetHealth.IsAlive;
    }
}