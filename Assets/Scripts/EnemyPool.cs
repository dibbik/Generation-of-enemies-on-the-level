using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        public Enemy Prefab;
        public int PoolSize = 10;
    }

    [Header("Настройки пула")]
    [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _maxTotalEnemies = 30;

    private Dictionary<Enemy, Queue<GameObject>> _pools = new Dictionary<Enemy, Queue<GameObject>>();
    private Dictionary<GameObject, Enemy> _prefabToPoolMap = new Dictionary<GameObject, Enemy>();
    private int _totalSpawnedCount;

    private void Awake()
    {
        InitializePools();
    }

    public GameObject GetEnemy(Enemy prefab, Vector3 position, Quaternion rotation)
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

            if (enemy.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(-health.MaxHealth);
            }
        }

        return enemy;
    }

    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab != null && prefab.TryGetComponent(out Enemy enemyComponent))
        {
            return GetEnemy(enemyComponent, position, rotation);
        }

        return null;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        if (enemy == null) 
            return;

        Enemy prefab = GetPrefabForEnemy(enemy);
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
            if (config.Prefab != null)
            {
                var queue = new Queue<GameObject>();

                for (int i = 0; i < config.PoolSize; i++)
                {
                    GameObject enemy = CreateEnemy(config.Prefab);
                    queue.Enqueue(enemy);
                }

                _pools[config.Prefab] = queue;
            }
        }
    }

    private GameObject CreateEnemy(Enemy prefab)
    {
        if (prefab == null) 
            return null;

        GameObject enemy = Instantiate(prefab.gameObject, _poolContainer);
        enemy.SetActive(false);
        _prefabToPoolMap[enemy] = prefab;

        return enemy;
    }

    private Enemy GetPrefabForEnemy(GameObject enemy)
    {
        return _prefabToPoolMap.ContainsKey(enemy) ? _prefabToPoolMap[enemy] : null;
    }
}
