using Managers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSoundUI: MonoBehaviour
    {
        [Inject] private ISoundPlayer _soundPlayer;

        [SerializeField] public AudioClip audioClip;
        
        private void Start()
        {
            if(!audioClip)
                return;
            
            var button = GetComponent<Button>();
            
            button.onClick.AddListener(() =>
            {
                _soundPlayer.PlaySoundGlobal(audioClip, SoundType.UI);
            });
        }
    }
}