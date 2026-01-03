using System;
using DG.Tweening;
using UnityEngine;

namespace Visual
{
    public sealed class PaymentVisual : MonoBehaviour, IFreeable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        private Tween _moveTween;
        private Action _freeCallback;

        public void Setup(Sprite sprite, Transform target)
        {
            if (spriteRenderer == null)
                throw new MissingReferenceException(nameof(spriteRenderer));

            if (target == null)
                throw new MissingReferenceException(nameof(target));

            spriteRenderer.sprite = sprite;

            _moveTween?.Kill();

            _moveTween = transform
                .DOMove(target.position, moveDuration)
                .SetEase(ease)
                .SetUpdate(true)
                .SetAutoKill(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(_freeCallback.Invoke);
        }

        private void OnDisable()
        {
            _moveTween?.Kill();
        }

        public void Deinitialize()
        {
            _moveTween?.Kill();
            _freeCallback = null;
        }

        public void Initialize(Action free)
        {
            _freeCallback = free;
        }
    }
}