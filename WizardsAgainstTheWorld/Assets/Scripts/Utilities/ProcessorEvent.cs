using System;
using System.Collections.Generic;

namespace Utilities
{
    public class ProcessorEvent<TContext>
    {
        private readonly SortedList<int, List<Action<TContext>>> _handlers = new();

        public void Register(Action<TContext> handler, int priority = 0)
        {
            if (!_handlers.ContainsKey(priority))
                _handlers[priority] = new List<Action<TContext>>();

            _handlers[priority].Add(handler);
        }

        public void Unregister(Action<TContext> handler)
        {
            foreach (var kvp in _handlers)
                kvp.Value.Remove(handler);
        }

        public void Invoke(TContext context)
        {
            foreach (var kvp in _handlers)
            {
                foreach (var handler in kvp.Value)
                {
                    handler(context);
                }
            }
        }
    }

}