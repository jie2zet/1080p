namespace Core.Framework;

public abstract class Singleton<T> where T : class, new()
{
    private static class SingletonHolder
    {
        internal static readonly T SingletonInstance = new T();
    }
    
    public static T Instance => SingletonHolder.SingletonInstance;
    
    protected Singleton() { }
}