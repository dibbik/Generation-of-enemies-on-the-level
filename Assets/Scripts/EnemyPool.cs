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

    private const int DefaultMaxTotalEnemies = 30;

    [Header("Настройки пула")]
    [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _maxTotalEnemies = DefaultMaxTotalEnemies;

    private Dictionary<Enemy, Queue<Enemy>> _pools = new Dictionary<Enemy, Queue<Enemy>>();
    private Dictionary<Enemy, Enemy> _prefabToPoolMap = new Dictionary<Enemy, Enemy>();
    private int _totalSpawnedCount;

    private void Awake()
    {
        InitializePools();
    }

    public Enemy GetEnemy(Enemy prefab, Vector3 position, Quaternion rotation)
    {
        if (_totalSpawnedCount >= _maxTotalEnemies)
            return null;

        if (!_pools.ContainsKey(prefab))
            return null;

        var pool = _pools[prefab];
        Enemy enemy;

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
            enemy.gameObject.SetActive(true);

            _totalSpawnedCount++;

            if (enemy.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(-health.MaxHealth);
            }
        }

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        Enemy prefab = GetPrefabForEnemy(enemy);

        if (prefab != null && _pools.ContainsKey(prefab))
        {
            enemy.gameObject.SetActive(false);

            if (_poolContainer != null)
            {
                enemy.transform.SetParent(_poolContainer);
            }
            _pools[prefab].Enqueue(enemy);
            _totalSpawnedCount--;
        }
        else
        {
            Destroy(enemy.gameObject);
        }
    }

    private void InitializePools()
    {
        foreach (var config in _poolConfigs)
        {
            if (config.Prefab != null)
            {
                var queue = new Queue<Enemy>();

                for (int i = 0; i < config.PoolSize; i++)
                {
                    Enemy enemy = CreateEnemy(config.Prefab);
                    queue.Enqueue(enemy);
                }

                _pools[config.Prefab] = queue;
            }
        }
    }

    private Enemy CreateEnemy(Enemy prefab)
    {
        if (prefab == null)
            return null;

        Enemy enemy = Instantiate(prefab, _poolContainer);
        enemy.gameObject.SetActive(false);
        _prefabToPoolMap[enemy] = prefab;

        return enemy;
    }

    private Enemy GetPrefabForEnemy(Enemy enemy)
    {
        return _prefabToPoolMap.ContainsKey(enemy) ? _prefabToPoolMap[enemy] : null;
    }
}