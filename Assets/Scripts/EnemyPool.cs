using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int poolSize = 10;
    }

    [Header("Настройки пула")]
    [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _maxTotalEnemies = 30;

    private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> _prefabToPoolMap = new Dictionary<GameObject, GameObject>();
    private int _totalSpawnedCount;

    private void Awake()
    {
        InitializePools();
    }

    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (_totalSpawnedCount >= _maxTotalEnemies) 
            return null;

        if (!_pools.ContainsKey(prefab)) 
            return null;

        var pool = _pools[prefab];
        GameObject enemy;

        if (pool.Count > 0)
        {
            enemy = pool.Dequeue();
        }
        else
        {
            enemy = CreateEnemy(prefab);
        }

        if (enemy != null)
        {
            enemy.transform.position = position;
            enemy.transform.rotation = rotation;
            enemy.SetActive(true);

            _totalSpawnedCount++;

            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(-health.MaxHealth);
            }
        }

        return enemy;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        if (enemy == null) 
            return;

        GameObject prefab = GetPrefabForEnemy(enemy);
        if (prefab != null && _pools.ContainsKey(prefab))
        {
            enemy.SetActive(false);

            if (_poolContainer != null)
            {
                enemy.transform.SetParent(_poolContainer);
            }
            _pools[prefab].Enqueue(enemy);
            _totalSpawnedCount--;
        }
        else
        {
            Destroy(enemy);
        }
    }

    private void InitializePools()
    {
        foreach (var config in _poolConfigs)
        {
            if (config.prefab != null)
            {
                var queue = new Queue<GameObject>();

                for (int i = 0; i < config.poolSize; i++)
                {
                    GameObject enemy = CreateEnemy(config.prefab);
                    queue.Enqueue(enemy);
                }
                _pools[config.prefab] = queue;
            }
        }
    }

    private GameObject CreateEnemy(GameObject prefab)
    {
        if (prefab == null) 
            return null;

        GameObject enemy = Instantiate(prefab, _poolContainer);
        enemy.SetActive(false);
        _prefabToPoolMap[enemy] = prefab;

        return enemy;
    }

    private GameObject GetPrefabForEnemy(GameObject enemy)
    {
        return _prefabToPoolMap.ContainsKey(enemy) ? _prefabToPoolMap[enemy] : null;
    }
}