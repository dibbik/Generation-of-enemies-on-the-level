using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(CharacterAnimation))]
public class Enemy : MonoBehaviour
{
    private const float RespawnCheckInterval = 1f;

    private CharacterMovement _characterMovement;
    private HealthSystem _healthSystem;
    private AttackSystem _attackSystem;
    private CharacterAnimation _characterAnimation;
    private EnemyPool _enemyPool;
    private GameController _gameController;
    private Transform _target;
    private Hero _targetHeroPrefab;
    private float _lastRespawnCheckTime;

    public void Initialize(GameController gameController, Hero targetHeroPrefab)
    {
        _gameController = gameController;
        _targetHeroPrefab = targetHeroPrefab;
        _gameController?.RegisterEnemy(this);

        FindAssignedHero();
    }

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _healthSystem = GetComponent<HealthSystem>();
        _attackSystem = GetComponent<AttackSystem>();
        _characterAnimation = GetComponent<CharacterAnimation>();
        TryGetComponent(out _enemyPool);

        _healthSystem.DeathEvent += HandleDeath;
    }

    private void OnDestroy()
    {
        _gameController?.UnregisterEnemy(this);
    }

    private void Update()
    {
        if (!_healthSystem.IsAlive)
            return;

        if (_target == null || !IsTargetValid(_target))
        {
            WaitForHeroRespawn();
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

    private void WaitForHeroRespawn()
    {
        _characterMovement.StopMovement();
        _attackSystem.StopAttack();

        if (Time.time - _lastRespawnCheckTime >= RespawnCheckInterval)
        {
            FindAssignedHero();
            _lastRespawnCheckTime = Time.time;
        }
    }

    private void FindAssignedHero()
    {
        if (_targetHeroPrefab != null && _gameController != null)
        {
            _target = _gameController.FindHeroByPrefab(_targetHeroPrefab);
        }
        else
        {
            _target = null;
        }
    }

    private void ChaseBehavior()
    {
        _attackSystem.StopAttack();

        if (_target != null)
        {
            _characterMovement.SetMovementDirection(_target.position - transform.position);
        }
    }

    private void AttackBehavior()
    {
        _attackSystem.StartAttack();
        _characterMovement.StopMovement();

        if (_target != null)
        {
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            directionToTarget.y = 0;
            _characterMovement.SetForcedRotation(directionToTarget);

            if (_target.TryGetComponent(out HealthSystem targetHealth))
            {
                _attackSystem.PerformAttack(targetHealth);
            }
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
}