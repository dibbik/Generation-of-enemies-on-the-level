using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AttackSystem))]
[RequireComponent(typeof(TargetFinder))]
public class Enemy : MonoBehaviour
{
    private CharacterMovement _movement;
    private HealthSystem _health;
    private AttackSystem _attack;
    private TargetFinder _targetFinder;
    private EnemyPool _pool;
    private Transform _target;

    private void Awake()
    {
        _movement = GetComponent<CharacterMovement>();
        _health = GetComponent<HealthSystem>();
        _attack = GetComponent<AttackSystem>();
        _targetFinder = GetComponent<TargetFinder>();
        _pool = FindObjectOfType<EnemyPool>();

        _health.DeathEvent += HandleDeath;
    }

    private void Update()
    {
        if (!_health.IsAlive) 
            return;

        _target = _targetFinder.FindTarget(typeof(Hero));

        if (_target == null)
        {
            _movement.StopMovement();
            return;
        }

        float distance = Vector3.Distance(transform.position, _target.position);

        if (distance <= _attack.AttackRange)
        {
            AttackBehavior();
        }
        else
        {
            ChaseBehavior();
        }
    }

    private void HandleDeath()
    {
        _pool?.ReturnEnemy(gameObject);
    }

    private void ChaseBehavior()
    {
        _attack.StopAttack();
        _movement.SetMovementDirection(_target.position - transform.position);
    }

    private void AttackBehavior()
    {
        _attack.StartAttack();
        _movement.StopMovement();

        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        directionToTarget.y = 0;
        _movement.SetForcedRotation(directionToTarget);

        HealthSystem targetHealth = _target.GetComponent<HealthSystem>();
        _attack.PerformAttack(targetHealth);
    }
}