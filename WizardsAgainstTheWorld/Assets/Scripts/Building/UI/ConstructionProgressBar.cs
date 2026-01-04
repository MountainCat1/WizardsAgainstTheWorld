using UnityEngine;
using UnityEngine.UI;

namespace Building.UI
{
    public class ConstructionProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider;
        
        [SerializeField] private BuildingConstruction buildingConstruction;

        private void Start()
        {
            buildingConstruction.ProgressChanged += OnProgressChanged;
            OnProgressChanged();
        }

        private void OnProgressChanged()
        {
            progressSlider.value = buildingConstruction.Progress;
            
            progressSlider.gameObject.SetActive(buildingConstruction.Progress != 0);
        }
    }
}