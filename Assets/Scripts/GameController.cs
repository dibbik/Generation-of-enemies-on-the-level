using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [SerializeField] private HeroCoordinator _heroCoordinator;
    [SerializeField] private EnemySpawner _enemySpawner;

    private List<Hero> _allHeroes = new List<Hero>();
    private List<Enemy> _allEnemies = new List<Enemy>();

    private void Awake()
    {
        InitializeSystems();
    }

    private void InitializeSystems()
    {
        _heroCoordinator.Initialize(this);
        _enemySpawner.Initialize(this);
    }

    public void RegisterHero(Hero hero)
    {
        if (!_allHeroes.Contains(hero))
        {
            _allHeroes.Add(hero);
        }
    }

    public void UnregisterHero(Hero hero)
    {
        _allHeroes.Remove(hero);
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

    public int GetEnemiesCount()
    {
        _allEnemies.RemoveAll(enemy => enemy == null);
        return _allEnemies.Count;
    }

    public Transform FindHeroByPrefab(Hero prefab)
    {
        foreach (Hero hero in _allHeroes)
        {
            if (hero != null && IsHeroFromPrefab(hero, prefab) && hero.IsAlive)
            {
                return hero.transform;
            }
        }
        return null;
    }

    public void HandleHeroDeath(Hero hero)
    {
        _heroCoordinator.HandleHeroDeath(hero);
    }

    private bool IsHeroFromPrefab(Hero heroInstance, Hero heroPrefab)
    {
        return heroInstance.GetType() == heroPrefab.GetType();
    }
}