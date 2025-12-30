using Managers;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class PlaySoundAction : ScriptableAction
    {
        [Inject] private ISoundPlayer _soundPlayer;
     
        [SerializeField] private AudioClip clip;
        
        public override void Execute()
        {
            base.Execute();
            
            _soundPlayer.PlaySound(clip, transform.position);
        }
    }
}