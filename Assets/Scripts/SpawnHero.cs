using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnHero : MonoBehaviour
{
    [System.Serializable]
    public class HeroRespawnConfig
    {
        public GameObject heroPrefab;
        public Transform spawnPoint;
        public List<Transform> patrolRoute;
        public GameObject respawnCircle;
        public float respawnDelay = 3f;
        public float respawnCircleDuration = 4f;
    }

    [Header("Конфигурации героев")]
    [SerializeField] private List<HeroRespawnConfig> _heroConfigs = new List<HeroRespawnConfig>();
    [SerializeField] private HeroPool _heroPool;

    private Dictionary<GameObject, HeroRespawnConfig> _heroToConfigMap = new Dictionary<GameObject, HeroRespawnConfig>();
    private Dictionary<GameObject, Coroutine> _respawnCoroutines = new Dictionary<GameObject, Coroutine>();

    private void Start()
    {
        if (_heroPool == null) 
            return;

        SpawnInitialHeroes();
    }

    public void HeroDeath(GameObject hero)
    {
        if (_heroToConfigMap.ContainsKey(hero))
        {
            HeroRespawnConfig config = _heroToConfigMap[hero];

            if (_respawnCoroutines.ContainsKey(hero))
            {
                StopCoroutine(_respawnCoroutines[hero]);
            }

            Coroutine respawnCoroutine = StartCoroutine(RespawnHeroProcess(config, hero));
            _respawnCoroutines[hero] = respawnCoroutine;
        }
    }

    public void RegisterNewHero(GameObject hero, GameObject prefab)
    {
        foreach (var config in _heroConfigs)
        {
            if (config.heroPrefab == prefab)
            {
                _heroToConfigMap[hero] = config;
                SetupHeroPosition(hero, config);
                return;
            }
        }
    }

    private void SpawnInitialHeroes()
    {
        foreach (var config in _heroConfigs)
        {
            if (config.heroPrefab != null && config.spawnPoint != null)
            {
                SpawnSingleHero(config);
            }
        }
    }

    private void SpawnSingleHero(HeroRespawnConfig config)
    {
        GameObject hero = _heroPool.GetHero(config.heroPrefab);
        if (hero != null)
        {
            SetupHeroPosition(hero, config);
        }
    }

    private IEnumerator RespawnHeroProcess(HeroRespawnConfig config, GameObject deadHero)
    {
        if (config.respawnCircle != null && config.spawnPoint != null)
        {
            config.respawnCircle.transform.position = config.spawnPoint.position;
            config.respawnCircle.SetActive(true);
        }

        yield return new WaitForSeconds(config.respawnDelay);

        SpawnSingleHero(config);

        if (config.respawnCircle != null)
        {
            config.respawnCircle.SetActive(false);
        }

        _respawnCoroutines.Remove(deadHero);
    }

    private void SetupHeroPosition(GameObject hero, HeroRespawnConfig config)
    {
        if (config.spawnPoint == null) 
            return;

        hero.transform.position = config.spawnPoint.position;
        hero.transform.rotation = config.spawnPoint.rotation;

        Rigidbody heroRigidbody = hero.GetComponent<Rigidbody>();

        if (heroRigidbody != null)
        {
            heroRigidbody.velocity = Vector3.zero;
            heroRigidbody.angularVelocity = Vector3.zero;
        }

        Hero heroComponent = hero.GetComponent<Hero>();

        if (heroComponent != null && config.patrolRoute != null && config.patrolRoute.Count > 0)
        {
            heroComponent.SetPatrolRoute(config.patrolRoute);
        }

        HealthSystem health = hero.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(-health.MaxHealth);
        }
    }
}