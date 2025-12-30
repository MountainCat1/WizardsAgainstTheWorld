using UnityEngine;

namespace ScriptableActions
{
    public class PlayAnimationAction : ScriptableAction
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animationStateName; // Name of the animation state to play
        
        public override void Execute()
        {
            base.Execute();
            
            if (animator != null && !string.IsNullOrEmpty(animationStateName))
            {
                animator.Play(animationStateName);
            }
            else
            {
                GameLogger.LogWarning($"Animator or animationStateName not set on {nameof(PlayAnimationAction)} in {animator?.gameObject?.name ?? "unknown"}");
            }
        }
    }
}