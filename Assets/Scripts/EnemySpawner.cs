using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private LayerMask _groundLayer = 1;

    [Header("Точки спавна")]
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

    [Header("Пул врагов")]
    [SerializeField] private EnemyPool _enemyPool;

    private const float RayStartHeight = 10f;
    private const float RayLength = 20f;
    private const float GroundOffset = 0.1f;
    private const float MaxSpawnHeight = 1f;

    private float _spawnTimer;

    private void Start()
    {
        ValidateDependencies();
        AdjustSpawnPointsHeight();
    }

    private void Update()
    {
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = _spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        if (_spawnPoints.Count == 0 || _enemyPool == null) 
            return;

        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPosition = GetSpawnPositionOnGround(spawnPoint.position);
        Quaternion spawnRotation = spawnPoint.rotation;

        Enemy enemy = _enemyPool.GetEnemy(spawnPosition, spawnRotation);

        if (enemy != null)
        {
            enemy.Initialize(spawnPoint.forward);
        }
    }

    private Vector3 GetSpawnPositionOnGround(Vector3 spawnPointPosition)
    {
        RaycastHit hit;
        Vector3 rayStart = spawnPointPosition + Vector3.up * RayStartHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, RayLength, _groundLayer))
        {
            return hit.point + Vector3.up * GroundOffset;
        }

        return spawnPointPosition;
    }

    private void AdjustSpawnPointsHeight()
    {
        foreach (Transform spawnPoint in _spawnPoints)
        {
            Vector3 currentPosition = spawnPoint.position;

            if (currentPosition.y > MaxSpawnHeight)
            {
                Vector3 newPosition = GetSpawnPositionOnGround(currentPosition);
                spawnPoint.position = newPosition;
            }
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        int randomIndex = Random.Range(0, _spawnPoints.Count);
        return _spawnPoints[randomIndex];
    }

    private void ValidateDependencies()
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("Не назначены точки спавна! Перетащи Transform'ы точек в Inspector");
        }

        if (_enemyPool == null)
        {
            Debug.LogError("Не назначен EnemyPool!");
        }
    }
}