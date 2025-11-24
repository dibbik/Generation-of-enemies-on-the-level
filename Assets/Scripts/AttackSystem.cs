using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    private const float DefaultAttackCooldown = 1f;
    private const float DefaultAttackDuration = 0.1f;

    [Header("Настройки атаки")]
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackCooldown = DefaultAttackCooldown;
    [SerializeField] private float _attackDuration = DefaultAttackDuration;

    private float _lastAttackTime;
    private bool _isAttacking;
    private float _attackEndTime;

    public float AttackRange => _attackRange;
    public bool IsAttacking => _isAttacking;
    public bool CanAttack => Time.time - _lastAttackTime >= _attackCooldown;

    private void Update()
    {
        if (_isAttacking && Time.time >= _attackEndTime)
        {
            StopAttack();
        }
    }

    public void StartAttack()
    {
        _isAttacking = true;
        _attackEndTime = Time.time + _attackDuration;
    }

    public void StopAttack()
    {
        _isAttacking = false;
    }

    public void PerformAttack(HealthSystem targetHealth)
    {
        if (CanAttack && targetHealth != null && targetHealth.IsAlive)
        {
            targetHealth.TakeDamage(_attackDamage);
            _lastAttackTime = Time.time;
            StartAttack();
        }
    }
}