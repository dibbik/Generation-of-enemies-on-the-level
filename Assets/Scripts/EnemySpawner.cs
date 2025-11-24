using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройка спавна")]
    [SerializeField] private int _maxEnemiesOnMap = 10;
    [SerializeField] private EnemyPool _enemyPool;
    [SerializeField] private List<EnemySpawnPoint> _spawnPoints = new List<EnemySpawnPoint>();

    private GameController _gameController;
    private Dictionary<EnemySpawnPoint, float> _spawnTimers = new Dictionary<EnemySpawnPoint, float>();
    private int _currentEnemiesCount;

    public void Initialize(GameController gameController)
    {
        _gameController = gameController;
        InitializeTimers();
    }

    private void Update()
    {
        if (_gameController == null)
            return;

        UpdateEnemiesCount();

        foreach (var spawnPoint in _spawnPoints)
        {
            if (spawnPoint == null || spawnPoint.EnemyPrefab == null)
                continue;

            if (!_spawnTimers.ContainsKey(spawnPoint))
            {
                _spawnTimers[spawnPoint] = spawnPoint.SpawnCooldown;
            }

            _spawnTimers[spawnPoint] -= Time.deltaTime;

            if (_spawnTimers[spawnPoint] <= 0f && _currentEnemiesCount < _maxEnemiesOnMap)
            {
                SpawnEnemy(spawnPoint);
                _spawnTimers[spawnPoint] = spawnPoint.SpawnCooldown;
            }
        }
    }

    private void UpdateEnemiesCount()
    {
        if (_gameController != null)
        {
            _currentEnemiesCount = _gameController.GetEnemiesCount();
        }
        else
        {
            _currentEnemiesCount = 0;
        }
    }

    private void SpawnEnemy(EnemySpawnPoint spawnPoint)
    {
        if (_enemyPool == null || _gameController == null)
            return;

        Enemy enemy = _enemyPool.GetEnemy(spawnPoint.EnemyPrefab);

        if (enemy != null)
        {
            enemy.transform.position = spawnPoint.Position;
            enemy.transform.rotation = Quaternion.identity;
            enemy.gameObject.SetActive(true);

            enemy.Initialize(_gameController, spawnPoint.TargetHeroPrefab);

            
        }
    }

    private void InitializeTimers()
    {
        foreach (var spawnPoint in _spawnPoints)
        {
            if (spawnPoint != null)
            {
                _spawnTimers[spawnPoint] = Random.Range(0f, spawnPoint.SpawnCooldown);
            }
        }
    }
}