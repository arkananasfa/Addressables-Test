using UnityEngine;

public class MoveState : PlayerState
{
    public MoveState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.PlayAnimation("Run");
    }

    public override void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Якщо користувач перестає рухатись – переходимо в Idle
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            player.ChangeState(new IdleState(player));
            return;
        }
    }

    public override void Update()
    {
        // Обчислюємо напрямок руху та переміщуємо персонажа
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0, v).normalized;
        player.transform.Translate(direction * player.moveSpeed * Time.deltaTime, Space.World);
    }
}