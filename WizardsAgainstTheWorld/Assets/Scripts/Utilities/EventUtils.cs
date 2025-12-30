using System;

namespace Utilities
{
    public static class EventUtils
    {
        public static void SafeInvoke(this Action action)
        {
            if (action == null) return;

            foreach (Delegate handler in action.GetInvocationList())
            {
                try
                {
                    ((Action)handler)();
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"SafeInvoke: Exception in event handler: {ex.Message}");
                    GameLogger.LogException(ex);
                }
            }
        }

        public static void SafeInvoke<T>(this Action<T> action, T arg)
        {
            if (action == null) return;

            foreach (Delegate handler in action.GetInvocationList())
            {
                try
                {
                    ((Action<T>)handler)(arg);
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"SafeInvoke: Exception in event handler: {ex}");
                }
            }
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null) return;

            foreach (Delegate handler in action.GetInvocationList())
            {
                try
                {
                    ((Action<T1, T2>)handler)(arg1, arg2);
                }
                catch (Exception ex)
                {
                    GameLogger.LogError($"SafeInvoke: Exception in event handler: {ex}");
                }
            }
        }

        // Add more overloads as needed
    }
}