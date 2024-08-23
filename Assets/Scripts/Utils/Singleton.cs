using System;

namespace Utils
{
    public class Singleton<T> where T : class, new()
    {
        // Lazy initialization ensures that the instance is created only when needed
        private static readonly Lazy<T> s_instance = new Lazy<T>(() => new T());

        // Private constructor prevents direct instantiation
        private Singleton()
        {
        }

        // The static property provides global access to the singleton instance
        public static T Instance => s_instance.Value;
    }
}