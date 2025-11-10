using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private int _maxEnemiesOnMap = 10;
    [SerializeField] private float _globalSpawnCooldown = 1f;

    [Header("Ссылки")]
    [SerializeField] private EnemyPool _enemyPool;
    [SerializeField] private List<EnemySpawnPoint> _spawnPoints = new List<EnemySpawnPoint>();

    private Dictionary<EnemySpawnPoint, float> _spawnTimers = new Dictionary<EnemySpawnPoint, float>();
    private float _globalSpawnTimer;
    private int _currentEnemiesCount;

    private void Awake()
    {
        InitializeTimers();
    }

    private void Update()
    {
        UpdateSpawning();
        UpdateEnemiesCount();
    }

    private void InitializeTimers()
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            if (spawnPoint != null)
            {
                _spawnTimers[spawnPoint] = 0f;
            }
        }
    }

    private void UpdateEnemiesCount()
    {
        Enemy[] activeEnemies = FindObjectsOfType<Enemy>();
        _currentEnemiesCount = 0;

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy.gameObject.activeInHierarchy && enemy.GetComponent<HealthSystem>().IsAlive)
            {
                _currentEnemiesCount++;
            }
        }
    }

    private void UpdateSpawning()
    {
        _globalSpawnTimer -= Time.deltaTime;

        if (_globalSpawnTimer > 0f) 
            return;

        if (_currentEnemiesCount >= _maxEnemiesOnMap) 
            return;

        foreach (var spawnPoint in _spawnPoints)
        {
            if (spawnPoint == null || spawnPoint.EnemyPrefab == null) 
                continue;

            if (!_spawnTimers.ContainsKey(spawnPoint))
            {
                _spawnTimers[spawnPoint] = 0f;
            }

            _spawnTimers[spawnPoint] -= Time.deltaTime;

            if (_spawnTimers[spawnPoint] <= 0f)
            {
                SpawnEnemy(spawnPoint);
                _spawnTimers[spawnPoint] = spawnPoint.SpawnCooldown;
                _globalSpawnTimer = _globalSpawnCooldown;
                break;
            }
        }
    }

    private void SpawnEnemy(EnemySpawnPoint spawnPoint)
    {
        if (_enemyPool == null) 
            return;

        GameObject enemyObject = _enemyPool.GetEnemy(
            spawnPoint.EnemyPrefab,
            spawnPoint.Position,
            Quaternion.identity
        );
    }
}