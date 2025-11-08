using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _movementDirection;
    private float _movementSpeed;
    private bool _isMoving;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_isMoving && _rigidbody != null)
        {
            Vector3 velocity = _movementDirection * _movementSpeed;
            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;

            if (_movementDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_movementDirection);
            }
        }
    }

    public void StartMovement(Vector3 direction, float speed)
    {
        _movementDirection = direction.normalized;
        _movementSpeed = speed;
        _isMoving = true;

        if (_movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_movementDirection);
        }
    }

    public void StopMovement()
    {
        _isMoving = false;

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }
}