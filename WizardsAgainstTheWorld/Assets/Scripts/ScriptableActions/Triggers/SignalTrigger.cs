// using Managers;
// using UnityEngine;
// using Zenject;
//
// namespace Triggers
// {
//     public class SignalTrigger : TriggerBase
//     {
//         [Inject] private ISignalManager _signalManager;
//         
//         [SerializeField] private Signals signal;
//
//         protected override void Start()
//         {
//             base.Start();
//             
//             _signalManager.Signaled += OnSignal;
//         }
//
//         private void OnSignal(Signals calledSignal)
//         {
//             if (calledSignal == signal)
//             {
//                 RunActions();
//             }
//         }
//     }
// }