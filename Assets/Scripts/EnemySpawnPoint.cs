using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnCooldown = 3f;

    public GameObject EnemyPrefab => _enemyPrefab;
    public Vector3 Position => transform.position;
    public float SpawnCooldown => _spawnCooldown;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Position, 0.1f);
        if (_enemyPrefab != null)
        {
            Gizmos.DrawIcon(Position, "Enemy", true);
        }
    }
}