using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class CreatureCurrentWeaponDisplay : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        
        private Image _image;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            if (creature == null)
            {
                GameLogger.LogError("Host creature is not assigned in the inspector.");
                return;
            }

            creature.WeaponChanged += UpdateWeaponDisplay;
            
            UpdateWeaponDisplay();
        }

        private void UpdateWeaponDisplay()
        {
            if (creature.Weapon == null)
            {
                _image.sprite = null;
                _image.gameObject.SetActive(false);
                return;
            }
            
            _image.gameObject.SetActive(true);

            if (creature.Weapon.Icon != null)
            {
                _image.sprite = creature.Weapon.Icon;
            }
            else
            {
                GameLogger.LogWarning("Weapon sprite is not assigned.");
                _image.sprite = null;
            }
        }
    }
}