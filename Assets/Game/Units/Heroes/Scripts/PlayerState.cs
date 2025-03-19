using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    // Викликається при вході в стан
    public virtual void Enter() { }
    // Викликається при виході зі стану
    public virtual void Exit() { }
    // Обробка вводу користувача
    public virtual void HandleInput() { }
    // Оновлення логіки стану кожного кадру
    public virtual void Update() { }
}