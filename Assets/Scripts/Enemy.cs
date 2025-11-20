using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(TargetFinder))]
[RequireComponent(typeof(CharacterAnimation))]
public class Enemy : MonoBehaviour
{
    private const float RespawnCheckInterval = 1f;

    private CharacterMovement _characterMovement;
    private HealthSystem _healthSystem;
    private AttackSystem _attackSystem;
    private TargetFinder _targetFinder;
    private CharacterAnimation _characterAnimation;
    private EnemyPool _enemyPool;
    private GameController _gameController;
    private Transform _target;
    private Transform _forcedTarget;
    private bool _waitingForRespawn;
    private float _lastRespawnCheckTime;
    private Hero _targetHeroPrefab;

    public void Initialize(GameController gameController, Hero targetHeroPrefab)
    {
        _gameController = gameController;
        _targetHeroPrefab = targetHeroPrefab;
        _gameController?.RegisterEnemy(this);
    }

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _healthSystem = GetComponent<HealthSystem>();
        _attackSystem = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        _characterAnimation = GetComponent<CharacterAnimation>();
        TryGetComponent(out _enemyPool);

        _healthSystem.DeathEvent += HandleDeath;

        if (TryGetComponent(out HealthSystem health))
        {
            TargetRegistry.Instance?.RegisterTarget(health);
        }
    }

    private void OnDestroy()
    {
        _gameController?.UnregisterEnemy(this);
    }

    public void SetForcedTarget(Transform target, Hero heroPrefab = null)
    {
        if (target != null)
        {
            _forcedTarget = target;
            _targetHeroPrefab = heroPrefab;
            _waitingForRespawn = false;
            _target = _forcedTarget;
        }
    }

    private void Update()
    {
        if (!_healthSystem.IsAlive)
            return;

        if(_forcedTarget != null)
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
                UpdateAnimations();
                return;
            }
        }


        if (_target == null)
        {
            _characterMovement.StopMovement();
            UpdateAnimations();
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

        UpdateAnimations();
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
        if (_targetHeroPrefab != null && _gameController != null)
        {
            Transform hero = _gameController.FindHeroByPrefab(_targetHeroPrefab);
            if (hero != null)
            {
                _forcedTarget = hero.transform;
                _waitingForRespawn = false;
                _target = _forcedTarget;
                
            }
        }
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

    private void UpdateAnimations()
    {
        bool isMoving = _characterMovement.MovementDirection != Vector3.zero;
        bool isAttacking = _attackSystem.IsAttacking;

        _characterAnimation.SetMoving(isMoving && !isAttacking);
        _characterAnimation.SetAttacking(isAttacking);
    }

    private void HandleDeath()
    {
        _enemyPool?.ReturnEnemy(this);
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null)
            return false;

        return target.TryGetComponent(out HealthSystem targetHealth) && targetHealth.IsAlive;
    }

    public bool IsWaitingForRespawn()
    {
        return _waitingForRespawn;
    }
}