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

    private Dictionary<Enemy, Queue<Enemy>> _pools = new Dictionary<Enemy, Queue<Enemy>>();

    private void Awake()
    {
        InitializePools();
    }

    public Enemy GetEnemy(Enemy prefab)
    {
        if (!_pools.ContainsKey(prefab))
            return null;

        var pool = _pools[prefab];
        return pool.Count > 0 ? pool.Dequeue() : CreateEnemy(prefab);
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null) 
            return;

        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(_poolContainer);

        foreach (var kvp in _pools)
        {
            Enemy prefab = kvp.Key;
            Queue<Enemy> pool = kvp.Value;

            if (enemy.GetType() == prefab.GetType())
            {
                pool.Enqueue(enemy);
                return;
            }
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
        Enemy enemy = Instantiate(prefab, _poolContainer);
        enemy.gameObject.SetActive(false);
        return enemy;
    }
}