using UnityEngine;
using UnityEngine.UI;

namespace UI
{

    public class InteractionProgressBarUI : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Graphic graphic;

        private void Start()
        {
            creature.Interacted += OnInteracted;
            creature.InteractionCanceled += OnInteractionCanceled;
            
            progressBar.gameObject.SetActive(false);
        }

        private void OnInteractionCanceled(Interaction interaction)
        {
            progressBar.gameObject.SetActive(false);
            progressBar.value = 0;
        }

        private void OnInteracted(Interaction interaction)
        {
            UpdateProgressBar(interaction);
        }

        private void UpdateProgressBar(Interaction interaction)
        {
            progressBar.value = (float)interaction.CurrentProgress;
            progressBar.gameObject.SetActive(interaction.Status == InteractionStatus.InProgress);
            progressBar.maxValue = (float)interaction.InteractionTime;
            graphic.color = interaction.Color;
        }
    }

}
