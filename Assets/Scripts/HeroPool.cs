using UnityEngine;
using System.Collections.Generic;

public class HeroPool : MonoBehaviour
{
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _initialPoolSize = 2;

    private Dictionary<Hero, Queue<Hero>> _pools = new Dictionary<Hero, Queue<Hero>>();
    private Dictionary<Hero, Hero> _instanceToPrefabMap = new Dictionary<Hero, Hero>();
    private HeroCoordinator _heroCoordinator;

    private void Awake()
    {
        _heroCoordinator = GetComponent<HeroCoordinator>();
    }

    public Hero GetHero(Hero prefab)
    {
        if (prefab == null)
            return null;

        if (!_pools.ContainsKey(prefab))
        {
            InitializePoolForPrefab(prefab);
        }

        var pool = _pools[prefab];
        Hero hero;

        if (pool.Count > 0)
        {
            hero = pool.Dequeue();
        }
        else
        {
            hero = CreateHero(prefab);
        }

        if (hero != null)
        {
            hero.gameObject.SetActive(true);
            _heroCoordinator?.RegisterHero(hero, prefab);
        }

        return hero;
    }

    public void ReturnHero(Hero hero)
    {
        if (hero == null)
            return;

        Hero prefab = GetPrefabForHero(hero);

        if (prefab != null && _pools.ContainsKey(prefab))
        {
            hero.gameObject.SetActive(false);
            hero.transform.SetParent(_poolContainer);
            hero.transform.position = Vector3.zero;
            _pools[prefab].Enqueue(hero);
        }
    }

    private void InitializePoolForPrefab(Hero prefab)
    {
        var queue = new Queue<Hero>();

        for (int i = 0; i < _initialPoolSize; i++)
        {
            Hero hero = CreateHero(prefab);
            queue.Enqueue(hero);
        }
        _pools[prefab] = queue;
    }

    private Hero CreateHero(Hero prefab)
    {
        Hero hero = Instantiate(prefab, _poolContainer);
        hero.gameObject.SetActive(false);
        _instanceToPrefabMap[hero] = prefab;
        return hero;
    }

    private Hero GetPrefabForHero(Hero hero)
    {
        return _instanceToPrefabMap.ContainsKey(hero) ? _instanceToPrefabMap[hero] : null;
    }
}