using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class ImageAnimationUI : MonoBehaviour
    {
        [Header("Fade Settings")]
        public float fadeDuration = 1f;

        [Header("Sprite Animation Settings")]
        public List<Sprite> sprites;
        public float spriteChangeInterval = 0.5f;

        private Image _image;
        private Coroutine _fadeCoroutine;
        private Coroutine _spriteAnimationCoroutine;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void FadeIn()
        {
            StartFade(0f, 1f);
        }

        public void FadeOut()
        {
            StartFade(1f, 0f);
        }

        private void StartFade(float fromAlpha, float toAlpha)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeImage(fromAlpha, toAlpha));
        }

        private IEnumerator FadeImage(float fromAlpha, float toAlpha)
        {
            float elapsed = 0f;
            Color color = _image.color;
            while (elapsed < fadeDuration)
            {
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
                _image.color = new Color(color.r, color.g, color.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _image.color = new Color(color.r, color.g, color.b, toAlpha);
        }

        public void StartSpriteAnimation()
        {
            if (_spriteAnimationCoroutine != null)
                StopCoroutine(_spriteAnimationCoroutine);
            _spriteAnimationCoroutine = StartCoroutine(AnimateSprites());
        }

        public void StopSpriteAnimation()
        {
            if (_spriteAnimationCoroutine != null)
                StopCoroutine(_spriteAnimationCoroutine);
        }

        private IEnumerator AnimateSprites()
        {
            int index = 0;
            while (true)
            {
                if (sprites.Count > 0)
                {
                    _image.sprite = sprites[index];
                    index = (index + 1) % sprites.Count;
                }
                yield return new WaitForSeconds(spriteChangeInterval);
            }
        }
    }
}
