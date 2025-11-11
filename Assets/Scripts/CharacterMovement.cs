using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 10f;

    private Rigidbody _characterRigidbody;
    private Vector3 _movementDirection;
    private Vector3 _forcedRotationDirection;
    private bool _useForcedRotation;

    public float MoveSpeed => _moveSpeed;
    public Vector3 MovementDirection => _movementDirection;

    private void Awake()
    {
        _characterRigidbody = GetComponent<Rigidbody>();
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
        _characterRigidbody.velocity = new Vector3(0, _characterRigidbody.velocity.y, 0);
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
        _characterRigidbody.isKinematic = false;
        _characterRigidbody.useGravity = true;
        _characterRigidbody.freezeRotation = true;
        _characterRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _characterRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Move()
    {
        if (_movementDirection != Vector3.zero)
        {
            Vector3 velocity = _movementDirection * _moveSpeed;
            velocity.y = _characterRigidbody.velocity.y;
            _characterRigidbody.velocity = velocity;
        }
        else
        {
            _characterRigidbody.velocity = new Vector3(0, _characterRigidbody.velocity.y, 0);
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