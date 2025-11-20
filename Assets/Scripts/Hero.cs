using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(TargetFinder))]
[RequireComponent(typeof(CharacterAnimation))]
public class Hero : MonoBehaviour
{
    private const float TargetCheckInterval = 0.5f;

    [Header("Настройки патрулирования")]
    [SerializeField] private List<Transform> _patrolPoints = new List<Transform>();
    [SerializeField] private float _reachDistance = 0.5f;

    private CharacterMovement _characterMovement;
    private HealthSystem _healthSystem;
    private AttackSystem _attackSystem;
    private TargetFinder _targetFinder;
    private CharacterAnimation _characterAnimation;
    private GameController _gameController;
    private Transform _currentPatrolTarget;
    private Transform _attackTarget;
    private int _currentPatrolIndex;
    private float _lastTargetCheckTime;

    public bool IsAlive => _healthSystem != null && _healthSystem.IsAlive;

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _healthSystem = GetComponent<HealthSystem>();
        _attackSystem = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        _characterAnimation = GetComponent<CharacterAnimation>();

        _healthSystem.DeathEvent += HandleDeath;

        _gameController = FindObjectOfType<GameController>();
        _gameController?.RegisterHero(this);

        if (TryGetComponent(out HealthSystem health))
        {
            TargetRegistry.Instance?.RegisterTarget(health);
        }
    }

    private void OnDestroy()
    {
        _gameController?.UnregisterHero(this);
    }

    private void Update()
    {
        if (!_healthSystem.IsAlive)
            return;

        if (_patrolPoints.Count == 0 || _currentPatrolTarget == null)
        {
            _characterMovement.StopMovement();
            _attackSystem.StopAttack();
            UpdateAnimations();
            return;
        }

        if (Time.time - _lastTargetCheckTime >= TargetCheckInterval)
        {
            CheckForEnemies();
            _lastTargetCheckTime = Time.time;
        }

        if (_attackTarget != null)
        {
            float sqrDistanceToEnemy = (_attackTarget.position - transform.position).sqrMagnitude;
            float sqrAttackRange = _attackSystem.AttackRange * _attackSystem.AttackRange;

            if (sqrDistanceToEnemy <= sqrAttackRange)
            {
                AttackBehavior();
            }
            else
            {
                _attackSystem.StopAttack();
                MoveToPatrolPoint();
            }
        }
        else
        {
            _attackSystem.StopAttack();
            MoveToPatrolPoint();
        }

        UpdateAnimations();
    }

    public void SetPatrolRoute(List<Transform> patrolRoute)
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        _patrolPoints = new List<Transform>(patrolRoute);
        _currentPatrolIndex = 0;
        _currentPatrolTarget = _patrolPoints[0];
    }

    private void MoveToPatrolPoint()
    {
        Vector3 direction = (_currentPatrolTarget.position - transform.position).normalized;
        _characterMovement.SetMovementDirection(direction);

        float sqrDistance = (_currentPatrolTarget.position - transform.position).sqrMagnitude;
        float sqrReachDistance = _reachDistance * _reachDistance;

        if (sqrDistance <= sqrReachDistance)
        {
            SetNextPatrolPoint();
        }
    }

    private void CheckForEnemies()
    {
        _attackTarget = _targetFinder.FindTarget(typeof(Enemy));
    }

    private void SetNextPatrolPoint()
    {
        if (_patrolPoints.Count == 0)
            return;

        _currentPatrolIndex++;
        if (_currentPatrolIndex >= _patrolPoints.Count)
        {
            _currentPatrolIndex = 0;
        }
        _currentPatrolTarget = _patrolPoints[_currentPatrolIndex];
    }

    private void AttackBehavior()
    {
        _attackSystem.StartAttack();
        _characterMovement.StopMovement();

        if (_attackTarget != null)
        {
            Vector3 directionToEnemy = (_attackTarget.position - transform.position).normalized;
            directionToEnemy.y = 0;
            _characterMovement.SetForcedRotation(directionToEnemy);
        }

        if (_attackTarget.TryGetComponent(out HealthSystem enemyHealth))
        {
            _attackSystem.PerformAttack(enemyHealth);
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
        _gameController?.HandleHeroDeath(this);
    }

    public override int GetHashCode()
    {
        return gameObject.GetInstanceID();
    }

    public override bool Equals(object other)
    {
        return other is Hero hero && hero.gameObject.GetInstanceID() == gameObject.GetInstanceID();
    }
}