using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private string _walkParameter = "Walk";
    [SerializeField] private string _attackParameter = "Attack";

    private Animator _characterAnimator;
    private CharacterMovement _characterMovement;
    private AttackSystem _attackSystem;
    private Enemy _enemy;

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
        _characterMovement = GetComponent<CharacterMovement>();
        _attackSystem = GetComponent<AttackSystem>();
        TryGetComponent(out _enemy);
    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (_characterAnimator == null) return;

        bool isMoving = _characterMovement.MovementDirection != Vector3.zero;
        bool isAttacking = _attackSystem.IsAttacking;
        bool isWaiting = _enemy != null && _enemy.IsWaitingForRespawn();

        if (isWaiting)
        {
            _characterAnimator.SetBool(_walkParameter, false);
            _characterAnimator.SetBool(_attackParameter, false);
        }
        else
        {
            _characterAnimator.SetBool(_walkParameter, isMoving && !isAttacking);
            _characterAnimator.SetBool(_attackParameter, isAttacking);
        }
    }
}