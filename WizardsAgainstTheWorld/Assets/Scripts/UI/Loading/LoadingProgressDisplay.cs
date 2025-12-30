using Managers;
using TMPro;
using UnityEngine;
using Utilities;
using Zenject;

namespace UI.Loading
{
    public class LoadingProgressDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text progressText;

        
        [Inject] private ISceneLoader _sceneLoader;
        
        private void Start()
        {
            
        }

        private void Update()
        {
            progressText.text = $"{LocalizationHelper.L("UI.Loading.Progress", $"{_sceneLoader.GetSceneLoadingProgress() * 100:#00}")}";
        }
    }
}