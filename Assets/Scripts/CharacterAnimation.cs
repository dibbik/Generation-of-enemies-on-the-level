using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private string _walkParameter = "Walk";
    [SerializeField] private string _attackParameter = "Attack";

    private Animator _animator;
    private CharacterMovement _movement;
    private AttackSystem _attack;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<CharacterMovement>();
        _attack = GetComponent<AttackSystem>();
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        bool isMoving = _movement.MovementDirection != Vector3.zero;
        bool isAttacking = _attack.IsAttacking;

        _animator.SetBool(_walkParameter, isMoving && !isAttacking);
        _animator.SetBool(_attackParameter, isAttacking);
    }
}