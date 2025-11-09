using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Настройки врага")]
    [SerializeField] private float _movementSpeed = 3f;
    [SerializeField] private float _deathHeight = 0f;

    private EnemyMovement _movement;
    private EnemyAnimation _animation;
    private bool _isActive;

    public event System.Action<Enemy> OnReturnToPoolRequested;

    private void Awake()
    {
        _movement = GetComponent<EnemyMovement>();
        _animation = GetComponent<EnemyAnimation>();
    }

    private void Update()
    {
        if (_isActive && transform.position.y < _deathHeight)
        {
            RequestReturnToPool();
        }
    }

    public void Initialize(Vector3 direction)
    {
        _isActive = true;
        _movement.StartMovement(direction, _movementSpeed);
        _animation?.PlayWalkAnimation();
    }

    public void Deactivate()
    {
        _isActive = false;
        _movement.StopMovement();
    }

    private void RequestReturnToPool()
    {
        if (!_isActive) 
            return;

        Deactivate();
        OnReturnToPoolRequested?.Invoke(this);
    }
}