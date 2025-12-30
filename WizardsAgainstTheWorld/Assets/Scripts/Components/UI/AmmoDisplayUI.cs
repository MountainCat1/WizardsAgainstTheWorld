using System.Collections;
using TMPro;
using UnityEngine;

namespace Components.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AmmoDisplayUI : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        [SerializeField] private TMP_Text text;
        [SerializeField] private float visibleDuration = 2f;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine _hideCoroutine;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            creature.WeaponAttacked += OnWeaponAttacked;
            creature.WeaponChanged += OnWeaponChanged;
            creature.WeaponReloaded += OnWeaponReloaded;
        }

        private void OnDestroy()
        {
            if (!creature) return;

            creature.WeaponAttacked -= OnWeaponAttacked;
            creature.WeaponChanged -= OnWeaponChanged;
            creature.WeaponReloaded -= OnWeaponReloaded;
        }

        private void OnWeaponReloaded() => UpdateUI();
        private void OnWeaponChanged() => UpdateUI();
        private void OnWeaponAttacked(AttackContext attackContext) => UpdateUI();

        private void UpdateUI()
        {
            if (creature?.Weapon?.ReloadComponent == null)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            text.text = $"{creature.Weapon.ReloadComponent.CurrentAmmo}/{creature.Weapon.ReloadComponent.GetMaxAmmo()}";

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _hideCoroutine = StartCoroutine(ShowAndFade());
        }

        private IEnumerator ShowAndFade()
        {
            // Instantly show
            _canvasGroup.alpha = 1f;

            // Wait before fading
            yield return new WaitForSeconds(visibleDuration);

            // Fade out smoothly
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
        }
    }
}
