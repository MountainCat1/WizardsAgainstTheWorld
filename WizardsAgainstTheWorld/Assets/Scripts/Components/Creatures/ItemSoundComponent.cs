using Items;
using Managers;
using UnityEngine;
using Zenject;

namespace Components
{
    [RequireComponent(typeof(ActiveItemBehaviour))]
    public class ItemSoundComponent : MonoBehaviour
    {
    
        [Inject] ISoundPlayer _soundPlayer = null!;

        [SerializeField] private AudioClip useSound;
        
        private void Awake()
        {
            var item = GetComponent<ActiveItemBehaviour>();
            
            item.ActiveAbilityUsed += OnUsed;
        }

        private void OnUsed(AbilityUseContext ctx)
        {
            if (useSound == null)
                return;
            
            _soundPlayer.PlaySound(useSound, ctx.User.transform.position, SoundType.Sfx, true);
        }
    }
}