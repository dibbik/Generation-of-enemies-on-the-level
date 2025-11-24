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

    [Header("Конфигурация героя")]
    [SerializeField] private List<HeroConfig> _heroConfigs = new List<HeroConfig>();
    [SerializeField] private HeroPool _heroPool;

    private Dictionary<Hero, HeroConfig> _heroToConfigMap = new Dictionary<Hero, HeroConfig>();
    private Dictionary<Hero, Coroutine> _respawnCoroutines = new Dictionary<Hero, Coroutine>();
    private GameController _gameController;

    public void Initialize(GameController gameController)
    {
        _gameController = gameController;

        if (_heroPool == null)
            _heroPool = GetComponent<HeroPool>();

        SpawnInitialHeroes();
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
                return;
            }
        }
    }

    public void RegisterEnemy(Enemy enemy) { }
    public void UnregisterEnemy(Enemy enemy) { }

    public Hero GetPrefabForHero(Hero heroInstance)
    {
        return _heroToConfigMap.ContainsKey(heroInstance) ? _heroToConfigMap[heroInstance].HeroPrefab : null;
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
        if (_heroPool == null)
        {
            return;
        }

        Hero hero = _heroPool.GetHero(config.HeroPrefab);

        if (hero != null)
        {
            SetupHero(hero, config);
        }
    }

    private void SetupHero(Hero hero, HeroConfig config)
    {
        if (config.SpawnPoint == null)
        {
            return;
        }

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