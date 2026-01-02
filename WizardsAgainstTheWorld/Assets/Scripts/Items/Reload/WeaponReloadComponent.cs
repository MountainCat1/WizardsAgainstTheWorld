using System;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Zenject;

namespace Items.Reload
{
    [RequireComponent(typeof(Weapon))]
    public abstract class WeaponReloadComponent : MonoBehaviour
    {
        public event Action Reloaded;
        
        [field: SerializeField, ReadOnlyInInspector]
        public virtual int CurrentAmmo { get; set; }

        [field: FormerlySerializedAs("<MaxAmmo>k__BackingField")]
        [field: SerializeField]
        public virtual int BaseMaxAmmo { get; private set; }

        public virtual int GetMaxAmmo() => BaseMaxAmmo; 

        [field: SerializeField]
        public virtual float ReloadTime { get; set; }

        [SerializeField] 
        protected AudioClip reloadFinishSound;

        [SerializeField] 
        protected AudioClip reloadStartSound;

        [SerializeField] 
        protected AudioClip outOfAmmoSound;

        [Inject]
        private ISoundPlayer _soundPlayer = null!;

        protected Weapon Weapon { get; private set; }

        public virtual bool IsReloading { get; protected set; }
        public virtual bool CanReload => CurrentAmmo < GetMaxAmmo();

        protected virtual void Awake()
        {
            Weapon = GetComponent<Weapon>();
            if (Weapon == null)
            {
                GameLogger.LogError($"{nameof(WeaponReloadComponent)} requires a {nameof(Weapon)} component on the same GameObject.");
            }
            
            CurrentAmmo = BaseMaxAmmo;
        }

        public virtual void DoReloading(Entity reloader)
        {
            throw new System.NotImplementedException();
        }

        public virtual void ConsumeAmmo()
        {
            if (CurrentAmmo > 0)
            {
                CurrentAmmo--;
                return;
            }

            GameLogger.LogWarning("Out of ammo, cannot consume ammo.");
            if (outOfAmmoSound != null && _soundPlayer != null)
            {
                _soundPlayer.PlaySound(outOfAmmoSound, transform.position);
            }
        }
        
        protected void InvokeReloaded()
        {
            Reloaded?.Invoke();
        }
    }
}