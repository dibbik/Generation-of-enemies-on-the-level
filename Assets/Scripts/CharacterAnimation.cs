using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : MonoBehaviour
{
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Animator _characterAnimator;

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    public void SetMoving(bool isMoving)
    {
        _characterAnimator.SetBool(WalkHash, isMoving);
    }

    public void SetAttacking(bool isAttacking)
    {
        _characterAnimator.SetBool(AttackHash, isAttacking);
    }
}