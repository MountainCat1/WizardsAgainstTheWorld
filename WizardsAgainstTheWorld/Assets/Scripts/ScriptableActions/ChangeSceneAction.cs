using UnityEngine;
using Utilities;

namespace ScriptableActions
{
    public class ChangeSceneAction : ScriptableAction
    {
        [SerializeField] private SceneReference sceneName;

        public override void Execute()
        {
            base.Execute();

            if (!string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                GameLogger.LogWarning($"Scene name not set on {nameof(ChangeSceneAction)} in {gameObject.name}");
            }
        }
    }
}