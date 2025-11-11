using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(TargetFinder))]
public class Hero : MonoBehaviour
{
    [Header("Настройки патрулирования")]
    [SerializeField] private List<Transform> _patrolPoints = new List<Transform>();
    [SerializeField] private float _reachDistance = 0.5f;

    private CharacterMovement _characterMovement;
    private HealthSystem _healthSystem;
    private AttackSystem _attackSystem;
    private TargetFinder _targetFinder;
    private HeroCoordinator _heroCoordinator;
    private Transform _currentPatrolTarget;
    private Transform _attackTarget;
    private int _currentPatrolIndex;
    private float _lastTargetCheckTime;
    private const float TargetCheckInterval = 0.5f;

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _healthSystem = GetComponent<HealthSystem>();
        _attackSystem = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        _heroCoordinator = FindObjectOfType<HeroCoordinator>();

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

        if (_patrolPoints.Count == 0 || _currentPatrolTarget == null)
        {
            _characterMovement.StopMovement();
            _attackSystem.StopAttack();
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

    private void HandleDeath()
    {
        _heroCoordinator?.HandleHeroDeath(gameObject);
    }
}