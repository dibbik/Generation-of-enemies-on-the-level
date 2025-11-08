using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Настройки врага")]
    [SerializeField] private float _movementSpeed = 3f;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _deathHeight = 0f;

    private const string WalkAnimationParameter = "IsWalking";

    private EnemyMovement _movement;
    private EnemyPool _pool;

    private bool _isActive;

    private void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (_isActive && transform.position.y < _deathHeight)
        {
            ReturnToPool();
        }
    }

    public void SetPool(EnemyPool pool)
    {
        _pool = pool;
    }

    public void StartMoving()
    {
        _isActive = true;
        _movement.StartMovement(transform.forward, _movementSpeed);

        if (_animator != null)
        {
            _animator.SetBool(WalkAnimationParameter, true);
        }
    }

    private void ReturnToPool()
    {
        if (!_isActive) 
            return;

        _isActive = false;
        _movement.StopMovement();

        if (_animator != null)
        {
            _animator.SetBool(WalkAnimationParameter, false);
        }

        _pool?.ReturnEnemy(this);
    }
}