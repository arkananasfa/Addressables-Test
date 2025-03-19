using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.PlayAnimation("Idle");
    }

    public override void HandleInput()
    {
        // Відслідковуємо натискання клавіш WASD через Input.GetAxis
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            player.ChangeState(new MoveState(player));
        }
    }
}