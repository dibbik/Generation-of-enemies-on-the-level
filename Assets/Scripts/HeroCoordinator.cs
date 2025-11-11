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

    private Dictionary<GameObject, HeroConfig> _heroToConfigMap = new Dictionary<GameObject, HeroConfig>();
    private Dictionary<GameObject, Coroutine> _respawnCoroutines = new Dictionary<GameObject, Coroutine>();

    private void Start()
    {
        if (_heroPool == null) 
            return;

        SpawnInitialHeroes();
    }

    public void HandleHeroDeath(GameObject hero)
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

    public void RegisterHero(GameObject hero, Hero prefab)
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

    private void NotifyEnemiesAboutNewHero(GameObject hero)
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy && enemy.IsWaitingForRespawn())
            {
                enemy.UpdateForcedTarget(hero.transform);
            }
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

    private void SpawnHero(HeroConfig config)
    {
        GameObject hero = _heroPool.GetHero(config.HeroPrefab);

        if (hero != null)
        {
            SetupHero(hero, config);
        }
    }

    private IEnumerator ExecuteRespawnProcess(HeroConfig config, GameObject deadHero)
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

    private void SetupHero(GameObject hero, HeroConfig config)
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

        if (hero.TryGetComponent(out Hero heroComponent) && config.PatrolRoute != null && config.PatrolRoute.Count > 0)
        {
            heroComponent.SetPatrolRoute(config.PatrolRoute);
        }

        if (hero.TryGetComponent(out HealthSystem health))
        {
            health.TakeDamage(-health.MaxHealth);
        }
    }
}