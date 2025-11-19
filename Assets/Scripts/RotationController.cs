using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10f;

    public void UpdateRotation(Vector3 movementDirection, Vector3 forcedDirection, bool useForced)
    {
        Vector3 rotationDirection = useForced ? forcedDirection : movementDirection;

        if (rotationDirection != Vector3.zero)
        {
            RotateTowards(rotationDirection);
        }
    }

    public void RotateTowards(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}