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

    private CharacterMovement _movement;
    private HealthSystem _health;
    private AttackSystem _attack;
    private TargetFinder _targetFinder;
    private SpawnHero _spawnHero;
    private Transform _currentPatrolTarget;
    private Transform _attackTarget;
    private int _currentPatrolIndex;

    private void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _health = GetComponent<HealthSystem>();
        _attack = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        _spawnHero = FindObjectOfType<SpawnHero>();

        _health.DeathEvent += HandleDeath;
    }

    private void Update()
    {
        if (!_health.IsAlive) 
            return;

        if (_patrolPoints.Count == 0 || _currentPatrolTarget == null)
        {
            _movement.StopMovement();
            return;
        }

        CheckForEnemies();

        if (_attackTarget != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, _attackTarget.position);

            if (distanceToEnemy <= _attack.AttackRange)
            {
                AttackBehavior();
            }
            else
            {
                MoveToPatrolPoint();
            }
        }
        else
        {
            _attack.StopAttack();
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
        _movement.SetMovementDirection(direction);

        if (Vector3.Distance(transform.position, _currentPatrolTarget.position) <= _reachDistance)
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

        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        _currentPatrolTarget = _patrolPoints[_currentPatrolIndex];
    }

    private void AttackBehavior()
    {
        _attack.StartAttack();
        _movement.StopMovement();

        if (_attackTarget != null)
        {
            Vector3 directionToEnemy = (_attackTarget.position - transform.position).normalized;
            directionToEnemy.y = 0;
            _movement.SetForcedRotation(directionToEnemy);
        }

        HealthSystem enemyHealth = _attackTarget.GetComponent<HealthSystem>();
        _attack.PerformAttack(enemyHealth);
    }

    private void HandleDeath()
    {
        _spawnHero?.HeroDeath(gameObject);
    }
}