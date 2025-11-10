using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _attackCooldown = 1f;

    private float _lastAttackTime;
    private bool _isAttacking;

    public float AttackRange => _attackRange;
    public bool IsAttacking => _isAttacking;
    public bool CanAttack => Time.time - _lastAttackTime >= _attackCooldown;

    public void StartAttack()
    {
        _isAttacking = true;
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
        }
    }
}