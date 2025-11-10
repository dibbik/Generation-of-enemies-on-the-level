using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _rotationSpeed = 10f;

    private Rigidbody _rigidbody;
    private Vector3 _movementDirection;
    private Vector3 _forcedRotationDirection;
    private bool _useForcedRotation = false;

    public float Speed => _speed;
    public Vector3 MovementDirection => _movementDirection;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        SetupRigidbody();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    public void SetMovementDirection(Vector3 direction)
    {
        _movementDirection = direction.normalized;
        _useForcedRotation = false;
    }

    public void StopMovement()
    {
        _movementDirection = Vector3.zero;
        _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
    }

    public void SetForcedRotation(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            _forcedRotationDirection = direction.normalized;
            _useForcedRotation = true;
        }
    }

    private void SetupRigidbody()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Move()
    {
        if (_movementDirection != Vector3.zero)
        {
            Vector3 velocity = _movementDirection * _speed;
            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;
        }
        else
        {
            _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
        }
    }

    private void Rotate()
    {
        Vector3 rotationDirection = _useForcedRotation ? _forcedRotationDirection : _movementDirection;

        if (rotationDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}