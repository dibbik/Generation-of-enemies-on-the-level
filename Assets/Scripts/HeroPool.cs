using UnityEngine;
using System.Collections.Generic;

public class HeroPool : MonoBehaviour
{
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _initialPoolSize = 2;

    private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> _instanceToPrefabMap = new Dictionary<GameObject, GameObject>();
    private HeroCoordinator _heroCoordinator;

    private void Awake()
    {
        _heroCoordinator = FindObjectOfType<HeroCoordinator>();
    }

    public GameObject GetHero(GameObject prefab)
    {
        if (prefab == null) 
            return null;

        if (!_pools.ContainsKey(prefab))
        {
            InitializePoolForPrefab(prefab);
        }

        var pool = _pools[prefab];
        GameObject hero;

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
            hero.SetActive(true);
            _heroCoordinator?.RegisterHero(hero, prefab);
        }

        return hero;
    }

    public void ReturnHero(GameObject hero)
    {
        if (hero == null) 
            return;

        GameObject prefab = GetPrefabForHero(hero);

        if (prefab != null && _pools.ContainsKey(prefab))
        {
            hero.SetActive(false);
            hero.transform.SetParent(_poolContainer);
            hero.transform.position = Vector3.zero;
            _pools[prefab].Enqueue(hero);
        }
    }

    private void InitializePoolForPrefab(GameObject prefab)
    {
        var queue = new Queue<GameObject>();

        for (int i = 0; i < _initialPoolSize; i++)
        {
            GameObject hero = CreateHero(prefab);
            queue.Enqueue(hero);
        }

        _pools[prefab] = queue;
    }

    private GameObject CreateHero(GameObject prefab)
    {
        GameObject hero = Instantiate(prefab, _poolContainer);
        hero.SetActive(false);
        _instanceToPrefabMap[hero] = prefab;
        return hero;
    }

    private GameObject GetPrefabForHero(GameObject hero)
    {
        return _instanceToPrefabMap.ContainsKey(hero) ? _instanceToPrefabMap[hero] : null;
    }
}