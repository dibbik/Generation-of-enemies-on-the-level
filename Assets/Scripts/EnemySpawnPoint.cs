using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnCooldown = 3f;
    [SerializeField] private GameObject _targetHeroPrefab;

    public GameObject EnemyPrefab => _enemyPrefab;
    public Vector3 Position => transform.position;
    public float SpawnCooldown => _spawnCooldown;
    public GameObject TargetHeroPrefab => _targetHeroPrefab;

    public Transform FindTargetHero()
    {
        if (_targetHeroPrefab == null) 
            return null;

        Hero[] heroes = FindObjectsOfType<Hero>();

        foreach (Hero hero in heroes)
        {
            if (IsHeroFromPrefab(hero.gameObject, _targetHeroPrefab))
            {
                return hero.transform;
            }
        }

        return null;
    }

    private bool IsHeroFromPrefab(GameObject heroInstance, GameObject heroPrefab)
    {
        return heroInstance.name.StartsWith(heroPrefab.name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Position, 0.1f);

        if (_enemyPrefab != null)
        {
            Gizmos.DrawIcon(Position, "Enemy", true);
        }

        Transform hero = FindTargetHero();
        if (hero != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Position, hero.position);
        }
        else if (_targetHeroPrefab != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "Hero", true);
        }
    }
}