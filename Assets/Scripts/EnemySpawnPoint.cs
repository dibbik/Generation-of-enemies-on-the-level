using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private float _spawnCooldown = 3f;
    [SerializeField] private Hero _targetHeroPrefab;

    public Enemy EnemyPrefab => _enemyPrefab;
    public Vector3 Position => transform.position;
    public float SpawnCooldown => _spawnCooldown;
    public Hero TargetHeroPrefab => _targetHeroPrefab;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Position, 0.1f);

        if (_enemyPrefab != null)
        {
            Gizmos.DrawIcon(Position, "Enemy", true);
        }

        if (_targetHeroPrefab != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "Hero", true);
        }
    }
}