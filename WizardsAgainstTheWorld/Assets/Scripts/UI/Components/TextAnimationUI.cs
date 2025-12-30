using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextAnimationUI : MonoBehaviour
    {
        [SerializeField] private float delayBetweenFrames = 0.25f;
        [SerializeField] private List<string> textLines;    

        private TMP_Text _textComponent;
        private string _originalText;

        private Coroutine _animationCoroutine;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            if (textLines == null || textLines.Count == 0)
            {
                Debug.LogError("Text lines are not set or empty. Please provide valid text lines.");
                enabled = false; // Disable the component if no text lines are provided
                return;
            }
        }

        private void OnEnable()
        {
            if (_animationCoroutine == null)
            {
                _animationCoroutine = StartCoroutine(Animation());
            }
        }
        
        private void OnDisable()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            _textComponent.text = _originalText; // Reset text to original when disabled
        }


        private IEnumerator Animation()
        {
            while (true)
            {
                foreach (var line in textLines)
                {
                    _textComponent.text = line;
                    _originalText = line;
                    yield return new WaitForSeconds(delayBetweenFrames);
                }
            }
        }
    }
}