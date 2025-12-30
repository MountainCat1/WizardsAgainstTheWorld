using System;
using System.Collections.Generic;
using Managers;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Items.Reload
{
    public class StandardReloadComponent : WeaponReloadComponent, IInteractable
    {
        public Vector2 Position => transform.position;
        public bool IsInteractable => CanReload;
        public int Priority => throw new System.NotImplementedException();
        public ICollection<IInteractable> Container => throw new System.NotImplementedException();
        public override bool CanReload => CurrentAmmo < GetMaxAmmo();

        private float _reloadEndTime;

        private Interaction _interaction;

        [Inject] private ISoundPlayer _soundPlayer = null!;

        private void Start()
        {
            CurrentAmmo = GetMaxAmmo();
            IsReloading = false;
        }

        public override int GetMaxAmmo()
        {
            return Mathf.FloorToInt(Weapon.WeaponItemData.GetApplied(WeaponPropertyModifiers.AmmoCapacity, BaseMaxAmmo));
        }

        public bool CanInteract(Creature creature)
        {
            if (IsReloading) return false;
            if (CurrentAmmo <= 0) return false;

            return true;
        }

        public override void DoReloading(Creature reloader)
        {
            reloader.Interact(this);
        }

        public Interaction Interact(Creature creature, float deltaTime)
        {
            if (_interaction != null)
            {
                return _interaction.Progress((decimal)deltaTime);
            }
            
            var modifiedReloadTime = Weapon.WeaponItemData.GetApplied(WeaponPropertyModifiers.ReloadTime, ReloadTime);

            var interaction = new Interaction(creature, modifiedReloadTime, "Game.Interaction.Reload", Color.yellowNice);

            interaction.Completed += OnReloadInteractionCompleted;
            interaction.Canceled += OnInteractionCanceled;

            _interaction = interaction;
            IsReloading = true;

            if (reloadStartSound is not null)
            {
                _soundPlayer.PlaySound(reloadStartSound, Position);
            }

            return interaction;
        }

        private void OnInteractionCanceled()
        {
            IsReloading = false;
            _interaction = null;
        }

        private void OnReloadInteractionCompleted()
        {
            IsReloading = false;
            CurrentAmmo = GetMaxAmmo();
            _interaction = null;

            if (reloadFinishSound is not null)
            {
                _soundPlayer.PlaySound(reloadFinishSound, Position);
            }
            
            InvokeReloaded();
        }
    }
}