using System;

namespace Utilities
{
    public static class EventSafeInvoker
    {
        public static void InvokeSafe<T>(this EventHandler<T> evt, object sender, T args)
            where T : EventArgs
        {
            if (evt == null) return;

            foreach (var @delegate in evt.GetInvocationList())
            {
                var handler = (EventHandler<T>)@delegate;
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in event {evt.Method.Name} subscriber: {ex.Message}");
                }
            }
        }
    }

}