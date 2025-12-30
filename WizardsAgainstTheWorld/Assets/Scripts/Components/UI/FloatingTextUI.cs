using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class FloatingTextUI : MonoBehaviour, IFreeable
    {
        [field: SerializeField] private TextMeshProUGUI Text { get; set; }
        [field: SerializeField] public float FloatSpeed { get; private set; }
        [field: SerializeField] public float FloatTime { get; private set; }
        
        private IEnumerator _floatingTextCoroutine;
        private float _baseSize;
        private Action _freePoolObjectDelegate;
        private Tween _scaleTween;
        private void Awake()
        {
            _baseSize = Text.fontSize;
        }

        public void Setup(string text, Color color, float size = 1f, FontStyles style = FontStyles.Normal)
        {
            Text.text = text;
            Text.color = color;
            Text.fontSize = _baseSize * size;
            Text.fontStyle = style;
        }
        
        public void Run()
        {
            if (_scaleTween != null && _scaleTween.IsActive()) _scaleTween.Kill();

            _floatingTextCoroutine = FloatingTextCoroutine();

            // Reset scale before animating to ensure consistent behavior
            Text.transform.localScale = Vector3.zero;

            var sequence = DOTween.Sequence();  
            _scaleTween = Text.transform.DOScale(Vector3.one * 1.5f, FloatTime / 4f).SetEase(Ease.OutBack).SetUpdate(true);
            sequence.Append(_scaleTween);

            StartCoroutine(_floatingTextCoroutine);
        }

        
        public void Initialize(Action free)
        {
            _freePoolObjectDelegate = free;
        }

        private IEnumerator FloatingTextCoroutine()
        {
            var time = 0f;
            var startPosition = transform.position;
            var targetPosition = startPosition + new Vector3(0, 1, 0);
            while (time < FloatTime)
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, time / FloatTime);
                yield return null;
            }
            
            _freePoolObjectDelegate();
        }

        public void Deinitialize()
        {
        }
    }
}