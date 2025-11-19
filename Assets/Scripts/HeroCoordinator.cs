using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HeroCoordinator : MonoBehaviour
{
    [System.Serializable]
    public class HeroConfig
    {
        public Hero HeroPrefab;
        public Transform SpawnPoint;
        public List<Transform> PatrolRoute;
        public GameObject RespawnEffect;
        public float RespawnDelay = 3f;
    }

    private const float DefaultRespawnDelay = 3f;

    [Header("Конфигурация героя")]
    [SerializeField] private List<HeroConfig> _heroConfigs = new List<HeroConfig>();
    [SerializeField] private HeroPool _heroPool;

    private Dictionary<Hero, HeroConfig> _heroToConfigMap = new Dictionary<Hero, HeroConfig>();
    private Dictionary<Hero, Coroutine> _respawnCoroutines = new Dictionary<Hero, Coroutine>();
    private List<Enemy> _allEnemies = new List<Enemy>();

    private void Start()
    {
        if (_heroPool == null)
            _heroPool = GetComponent<HeroPool>();

        SpawnInitialHeroes();
        CacheAllEnemies();
    }

    public Hero GetPrefabForHero(Hero heroInstance)
    {
        if (_heroToConfigMap.ContainsKey(heroInstance))
        {
            return _heroToConfigMap[heroInstance].HeroPrefab;
        }
        return null;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!_allEnemies.Contains(enemy))
        {
            _allEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        _allEnemies.Remove(enemy);
    }

    public void HandleHeroDeath(Hero hero)
    {
        if (_heroToConfigMap.ContainsKey(hero))
        {
            HeroConfig config = _heroToConfigMap[hero];

            if (_respawnCoroutines.ContainsKey(hero))
            {
                StopCoroutine(_respawnCoroutines[hero]);
            }

            Coroutine respawnCoroutine = StartCoroutine(ExecuteRespawnProcess(config, hero));
            _respawnCoroutines[hero] = respawnCoroutine;
        }
    }

    public void RegisterHero(Hero hero, Hero prefab)
    {
        foreach (var config in _heroConfigs)
        {
            if (config.HeroPrefab == prefab)
            {
                _heroToConfigMap[hero] = config;
                SetupHero(hero, config);
                NotifyEnemiesAboutNewHero(hero);
                return;
            }
        }
    }

    private void NotifyEnemiesAboutNewHero(Hero hero)
    {
        for (int i = _allEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _allEnemies[i];
            if (enemy == null)
            {
                _allEnemies.RemoveAt(i);
                continue;
            }

            if (enemy.gameObject.activeInHierarchy && enemy.IsWaitingForRespawn())
            {
                enemy.UpdateForcedTarget(hero.transform);
            }
        }
    }

    private void CacheAllEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        _allEnemies.Clear();
        _allEnemies.AddRange(enemies);

        foreach (Enemy enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
    }

    private void SpawnInitialHeroes()
    {
        foreach (var config in _heroConfigs)
        {
            if (config.HeroPrefab != null && config.SpawnPoint != null)
            {
                SpawnHero(config);
            }
        }
    }

    private IEnumerator ExecuteRespawnProcess(HeroConfig config, Hero deadHero)
    {
        if (config.RespawnEffect != null && config.SpawnPoint != null)
        {
            config.RespawnEffect.transform.position = config.SpawnPoint.position;
            config.RespawnEffect.SetActive(true);
        }

        yield return new WaitForSeconds(config.RespawnDelay);

        SpawnHero(config);

        if (config.RespawnEffect != null)
        {
            config.RespawnEffect.SetActive(false);
        }

        _respawnCoroutines.Remove(deadHero);
    }

    private void SpawnHero(HeroConfig config)
    {
        Hero hero = _heroPool.GetHero(config.HeroPrefab);

        if (hero != null)
        {
            SetupHero(hero, config);
        }
    }

    private void SetupHero(Hero hero, HeroConfig config)
    {
        if (config.SpawnPoint == null)
            return;

        hero.transform.position = config.SpawnPoint.position;
        hero.transform.rotation = config.SpawnPoint.rotation;

        if (hero.TryGetComponent(out Rigidbody heroRigidbody))
        {
            heroRigidbody.velocity = Vector3.zero;
            heroRigidbody.angularVelocity = Vector3.zero;
        }

        if (config.PatrolRoute != null && config.PatrolRoute.Count > 0)
        {
            hero.SetPatrolRoute(config.PatrolRoute);
        }

        if (hero.TryGetComponent(out HealthSystem health))
        {
            health.TakeDamage(-health.MaxHealth);
        }
    }
}