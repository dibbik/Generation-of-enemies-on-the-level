using UnityEngine;
using System.Collections.Generic;

public class HeroRegistry : MonoBehaviour
{
    private static HeroRegistry _instance;
    private Dictionary<Hero, Hero> _heroes = new Dictionary<Hero, Hero>();
    private Dictionary<Hero, List<EnemySpawnPoint>> _spawnPointsByPrefab = new Dictionary<Hero, List<EnemySpawnPoint>>();

    public static HeroRegistry Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void RegisterHero(Hero heroInstance, Hero heroPrefab)
    {
        if (!_heroes.ContainsKey(heroInstance))
        {
            _heroes[heroInstance] = heroPrefab;
        }
    }

    public void UnregisterHero(Hero heroInstance)
    {
        _heroes.Remove(heroInstance);
    }

    public void RegisterSpawnPoint(EnemySpawnPoint spawnPoint, Hero targetPrefab)
    {
        if (targetPrefab == null) 
            return;

        if (!_spawnPointsByPrefab.ContainsKey(targetPrefab))
        {
            _spawnPointsByPrefab[targetPrefab] = new List<EnemySpawnPoint>();
        }

        if (!_spawnPointsByPrefab[targetPrefab].Contains(spawnPoint))
        {
            _spawnPointsByPrefab[targetPrefab].Add(spawnPoint);
        }
    }

    public void UnregisterSpawnPoint(EnemySpawnPoint spawnPoint, Hero targetPrefab)
    {
        if (targetPrefab != null && _spawnPointsByPrefab.ContainsKey(targetPrefab))
        {
            _spawnPointsByPrefab[targetPrefab].Remove(spawnPoint);
        }
    }

    public Transform FindHeroByPrefab(Hero targetPrefab)
    {
        foreach (var kvp in _heroes)
        {
            Hero heroInstance = kvp.Key;
            Hero heroPrefab = kvp.Value;

            if (heroPrefab == targetPrefab &&
                heroInstance != null &&
                heroInstance.gameObject.activeInHierarchy &&
                heroInstance.TryGetComponent(out HealthSystem health) &&
                health.IsAlive)
            {
                return heroInstance.transform;
            }
        }

        return null;
    }

    public List<Transform> GetAllHeroesByPrefab(Hero targetPrefab)
    {
        List<Transform> heroes = new List<Transform>();

        foreach (var kvp in _heroes)
        {
            Hero heroInstance = kvp.Key;
            Hero heroPrefab = kvp.Value;

            if (heroPrefab == targetPrefab &&
                heroInstance != null &&
                heroInstance.gameObject.activeInHierarchy &&
                heroInstance.TryGetComponent(out HealthSystem health) &&
                health.IsAlive)
            {
                heroes.Add(heroInstance.transform);
            }
        }

        return heroes;
    }
}