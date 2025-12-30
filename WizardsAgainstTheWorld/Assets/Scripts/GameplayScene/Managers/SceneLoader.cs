using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Managers
{
    public static class Scenes
    {
        public static string MainMenu => "Menu";
        public static string LevelSelector => "Level Select";
        public static string GameplayScene => "Game";
        public static string Intro => "Intro";
        public static string Outro => "Outro";
    }

    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
        void LoadScene(SceneReference sceneRef);
        void PreloadScene(string sceneName);
        void PreloadScene(SceneReference sceneRef);
        void ActivatePreloadedScene();
        void UnloadPreloadedScene();
        void SwitchToPreloadedScene();
        float GetSceneLoadingProgress();
    }

    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private float fakeMinDuration = 1f;

        private AsyncOperation _preloadOperation;
        private string _preloadedSceneName;
        private float _sceneLoadingProgress;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
            else
                GameLogger.LogWarning(
                    "Loading screen is not assigned in SceneLoader. Loading screen will not be shown.");
        }

        public float GetSceneLoadingProgress()
        {
            return _sceneLoadingProgress;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        public void LoadScene(SceneReference sceneRef)
        {
            LoadScene(sceneRef.ScenePath);
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;

            float timer = 0f;
            while (async.progress < 0.9f)
            {
                _sceneLoadingProgress = async.progress;
                timer += Time.deltaTime;
                yield return null;
            }

            _sceneLoadingProgress = 1f; // Ensure progress is set to 100% when loading is complete

            while (timer < fakeMinDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            async.allowSceneActivation = true;

            yield return new WaitUntil(() => async.isDone);
            yield return null;

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        public void PreloadScene(string sceneName)
        {
            if (_preloadOperation != null)
            {
                GameLogger.LogWarning("Another scene is already being preloaded.");
                return;
            }

            _preloadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _preloadOperation.allowSceneActivation = false;
            _preloadedSceneName = sceneName;

            GameLogger.Log($"Started preloading scene: {sceneName}");
        }

        public void PreloadScene(SceneReference sceneRef)
        {
            PreloadScene(sceneRef.ScenePath);
        }

        public void ActivatePreloadedScene()
        {
            if (_preloadOperation == null)
            {
                GameLogger.LogWarning("No preloaded scene to activate.");
                return;
            }

            StartCoroutine(ActivatePreloadedSceneCoroutine());
        }

        private IEnumerator ActivatePreloadedSceneCoroutine()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            _preloadOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => _preloadOperation.isDone);
            yield return null;

            if (loadingScreen != null)
                loadingScreen.SetActive(false);

            GameLogger.Log($"Activated preloaded scene: {_preloadedSceneName}");
            _preloadOperation = null;
            _preloadedSceneName = null;
        }

        public void UnloadPreloadedScene()
        {
            if (string.IsNullOrEmpty(_preloadedSceneName))
            {
                GameLogger.LogWarning("No preloaded scene to unload.");
                return;
            }

            SceneManager.UnloadSceneAsync(_preloadedSceneName);
            GameLogger.Log($"Unloaded preloaded scene: {_preloadedSceneName}");
            _preloadOperation = null;
            _preloadedSceneName = null;
        }

        public void SwitchToPreloadedScene()
        {
            if (_preloadOperation == null || string.IsNullOrEmpty(_preloadedSceneName))
            {
                GameLogger.LogWarning("No preloaded scene available to switch to.");
                return;
            }

            StartCoroutine(SwitchToPreloadedSceneCoroutine());
        }

        private IEnumerator SwitchToPreloadedSceneCoroutine()
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            _preloadOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => _preloadOperation.isDone);
            yield return null;

            var newScene = SceneManager.GetSceneByName(_preloadedSceneName);

            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
                GameLogger.Log($"Switched to preloaded scene: {_preloadedSceneName}");

                var previousScene = SceneManager.GetActiveScene(); // now it's the preloaded one
                foreach (var scene in SceneManager.GetAllScenes())
                {
                    if (scene.name != _preloadedSceneName && scene.isLoaded)
                    {
                        SceneManager.UnloadSceneAsync(scene);
                        GameLogger.Log($"Unloaded previous scene: {scene.name}");
                        break;
                    }
                }
            }
            else
            {
                GameLogger.LogError($"Failed to switch to scene: {_preloadedSceneName}");
            }

            _preloadOperation = null;
            _preloadedSceneName = null;

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
    }
}