using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private int _isWalkingHash;

    private void Awake()
    {
        _isWalkingHash = Animator.StringToHash("IsWalking");

        if (_animator == null)
        {
            Debug.LogWarning("Аниматор не назначен");
        }
    }

    public void PlayWalkAnimation()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
    }
}