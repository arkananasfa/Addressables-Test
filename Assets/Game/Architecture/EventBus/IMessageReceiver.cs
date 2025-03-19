public interface IMessageReceiver<T>
{
    // Метод, який буде викликано, коли EventBus публікує повідомлення типу T.
    void GotMessage(T context);
}