using System;
using System.Collections.Generic;
using Zenject;

namespace Managers
{
    public interface ISignalManager
    {
        event Action<Signal> Signaled;
        void Signal(Signal signal);
        int GetSignalCount(Signal signal);
    }


    public class SignalManager : ISignalManager
    {
        [Inject] private ISpawnerManager _spawnerManager;
        
        private readonly Dictionary<Signal, int > _signalCounts = new();
        
        public event Action<Signal> Signaled;

        public void Signal(Signal signal)
        {
            GameLogger.Log($"Signal {signal} called");
            
            _signalCounts.TryAdd(signal, 0);
            _signalCounts[signal]++;
            
            Signaled?.Invoke(signal);
        }

        public int GetSignalCount(Signal signal)
        {
            return _signalCounts.GetValueOrDefault(signal, 0);
        }
    }
}