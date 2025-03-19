using System;
using System.Collections.Generic;

public static class EventBus
{
    // Зберігаємо підписників за типом повідомлення.
    private static readonly Dictionary<Type, List<object>> subscribers = new Dictionary<Type, List<object>>();

    // Метод для підписки на повідомлення типу T.
    public static void Subscribe<T>(IMessageReceiver<T> subscriber)
    {
        Type type = typeof(T);
        if (!subscribers.ContainsKey(type))
        {
            subscribers[type] = new List<object>();
        }
        subscribers[type].Add(subscriber);
    }

    // Метод для відписки від повідомлень типу T.
    public static void Unsubscribe<T>(IMessageReceiver<T> subscriber)
    {
        Type type = typeof(T);
        if (subscribers.ContainsKey(type))
        {
            subscribers[type].Remove(subscriber);
        }
    }

    // Метод для публікації повідомлення. Для кожного підписника типу T викликається метод GotMessage.
    public static void Publish<T>(T message)
    {
        Type type = typeof(T);
        if (subscribers.ContainsKey(type))
        {
            foreach (object sub in subscribers[type])
            {
                if (sub is IMessageReceiver<T> receiver)
                {
                    receiver.GotMessage(message);
                }
            }
        }
    }
}