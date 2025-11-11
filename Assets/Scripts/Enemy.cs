using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(TargetFinder))]
public class Enemy : MonoBehaviour
{
    private CharacterMovement _characterMovement;
    private HealthSystem _healthSystem;
    private AttackSystem _attackSystem;
    private TargetFinder _targetFinder;
    private EnemyPool _enemyPool;
    private Transform _target;
    private Transform _forcedTarget;
    private bool _waitingForRespawn;
    private float _lastRespawnCheckTime;
    private const float RespawnCheckInterval = 1f;

    public void SetForcedTarget(Transform target)
    {
        _forcedTarget = target;
        _waitingForRespawn = false;
        _target = _forcedTarget;
    }

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _healthSystem = GetComponent<HealthSystem>();
        _attackSystem = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        TryGetComponent(out _enemyPool);

        _healthSystem.DeathEvent += HandleDeath;

        if (TryGetComponent(out HealthSystem health))
        {
            TargetRegistry.Instance?.RegisterTarget(health);
        }
    }

    private void Update()
    {
        if (!_healthSystem.IsAlive) 
            return;

        if (_forcedTarget != null)
        {
            if (IsTargetValid(_forcedTarget))
            {
                _target = _forcedTarget;
                _waitingForRespawn = false;
            }
            else
            {
                _target = null;
                _waitingForRespawn = true;
                WaitForRespawn();
                return;
            }
        }
        else
        {
            _target = _targetFinder.FindTarget(typeof(Hero));
        }

        if (_target == null)
        {
            _characterMovement.StopMovement();
            return;
        }

        float sqrDistance = (_target.position - transform.position).sqrMagnitude;
        float sqrAttackRange = _attackSystem.AttackRange * _attackSystem.AttackRange;

        if (sqrDistance <= sqrAttackRange)
        {
            AttackBehavior();
        }
        else
        {
            ChaseBehavior();
        }
    }

    private void WaitForRespawn()
    {
        _attackSystem.StopAttack();
        _characterMovement.StopMovement();

        if (Time.time - _lastRespawnCheckTime >= RespawnCheckInterval)
        {
            CheckForRespawnedHero();
            _lastRespawnCheckTime = Time.time;
        }
    }

    private void CheckForRespawnedHero()
    {
        if (_forcedTarget != null)
        {
            Hero[] heroes = FindObjectsOfType<Hero>();

            foreach (Hero hero in heroes)
            {
                if (IsHeroFromSamePrefab(hero.gameObject, _forcedTarget.gameObject) &&
                    hero.TryGetComponent(out HealthSystem health) && health.IsAlive)
                {
                    _forcedTarget = hero.transform;
                    _waitingForRespawn = false;
                    _target = _forcedTarget;
                    break;
                }
            }
        }
    }

    private bool IsHeroFromSamePrefab(GameObject hero1, GameObject hero2)
    {
        return hero1.name.Split('(')[0] == hero2.name.Split('(')[0];
    }

    private void HandleDeath()
    {
        _enemyPool?.ReturnEnemy(gameObject);
    }

    private void ChaseBehavior()
    {
        _attackSystem.StopAttack();
        _characterMovement.SetMovementDirection(_target.position - transform.position);
    }

    private void AttackBehavior()
    {
        _attackSystem.StartAttack();
        _characterMovement.StopMovement();

        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        directionToTarget.y = 0;
        _characterMovement.SetForcedRotation(directionToTarget);

        if (_target.TryGetComponent(out HealthSystem targetHealth))
        {
            _attackSystem.PerformAttack(targetHealth);
        }
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) 
            return false;

        return target.TryGetComponent(out HealthSystem targetHealth) && targetHealth.IsAlive;
    }

    public void UpdateForcedTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            _forcedTarget = newTarget;
            _waitingForRespawn = false;
            _target = _forcedTarget;
        }
    }

    public bool IsWaitingForRespawn()
    {
        return _waitingForRespawn;
    }
}