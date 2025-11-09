using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    [Header("Настройки пула")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private Transform _poolContainer;

    private Queue<Enemy> _inactiveEnemies = new Queue<Enemy>();
    private List<Enemy> _activeEnemies = new List<Enemy>();

    public int ActiveCount => _activeEnemies.Count;
    public int InactiveCount => _inactiveEnemies.Count;
    public int TotalCount => ActiveCount + InactiveCount;

    private void Start()
    {
        if (_poolContainer == null)
            _poolContainer = transform;

        InitializePool();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAllEnemies();
    }

    public Enemy GetEnemy(Vector3 position, Quaternion rotation)
    {
        if (_inactiveEnemies.Count == 0)
        {
            CreateNewEnemy();
        }

        Enemy enemy = _inactiveEnemies.Dequeue();
        _activeEnemies.Add(enemy);

        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.gameObject.SetActive(true);

        enemy.OnReturnToPoolRequested += HandleEnemyReturnRequest;

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null) 
            return;

        if (_activeEnemies.Remove(enemy))
        {
            enemy.OnReturnToPoolRequested -= HandleEnemyReturnRequest;

            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(_poolContainer);
            enemy.transform.position = Vector3.zero;
            _inactiveEnemies.Enqueue(enemy);
        }
    }

    public void ReturnAllActiveEnemies()
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            ReturnEnemy(_activeEnemies[i]);
        }
    }

    public void ClearPool()
    {
        UnsubscribeFromAllEnemies();
        ReturnAllActiveEnemies();

        while (_inactiveEnemies.Count > 0)
        {
            Enemy enemy = _inactiveEnemies.Dequeue();
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        _activeEnemies.Clear();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewEnemy();
        }
    }

    private void CreateNewEnemy()
    {
        Enemy enemy = Instantiate(_enemyPrefab, _poolContainer);
        enemy.gameObject.SetActive(false);
        _inactiveEnemies.Enqueue(enemy);
    }

    private void HandleEnemyReturnRequest(Enemy enemy)
    {
        ReturnEnemy(enemy);
    }

    private void UnsubscribeFromAllEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                enemy.OnReturnToPoolRequested -= HandleEnemyReturnRequest;
        }

        foreach (var enemy in _inactiveEnemies)
        {
            if (enemy != null)
                enemy.OnReturnToPoolRequested -= HandleEnemyReturnRequest;
        }
    }
}