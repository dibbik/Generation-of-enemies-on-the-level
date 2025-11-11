using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    public event Action DeathEvent;

    [SerializeField] private int _maxHealth = 30;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _currentHealth > 0;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive)
            return;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        _currentHealth = Mathf.Min(_currentHealth + healAmount, _maxHealth);
    }

    private void Die()
    {
        DeathEvent?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
    }
}