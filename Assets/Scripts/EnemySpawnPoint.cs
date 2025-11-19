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

    public Transform FindTargetHero()
    {
        if (_targetHeroPrefab == null)
            return null;

        Hero[] heroes = FindObjectsOfType<Hero>();

        foreach (Hero hero in heroes)
        {
            if (hero.gameObject.activeInHierarchy &&
                IsHeroFromPrefab(hero, _targetHeroPrefab) &&
                hero.TryGetComponent(out HealthSystem health) &&
                health.IsAlive)
            {
                return hero.transform;
            }
        }

        return null;
    }

    private bool IsHeroFromPrefab(Hero heroInstance, Hero heroPrefab)
    {
        return heroInstance.GetType() == heroPrefab.GetType() &&
               heroInstance.gameObject.name.StartsWith(heroPrefab.gameObject.name);
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