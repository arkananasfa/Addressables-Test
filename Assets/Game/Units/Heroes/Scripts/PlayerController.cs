using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    private PlayerState currentState;

    private void Start()
    {
        // Стартуємо з Idle стану
        ChangeState(new IdleState(this));
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.HandleInput();
            currentState.Update();
        }
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }
}