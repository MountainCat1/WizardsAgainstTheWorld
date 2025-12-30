using UnityEngine;

namespace LevelSelector
{
    [CreateAssetMenu(fileName = "RegionType", menuName = "Custom/RegionType")]
    public class RegionType : ScriptableObject
    {
        [SerializeField] public string typeName;
        [SerializeField] public int minDifficulty = 0;
        [SerializeField] public int maxDifficulty = 0;
        [SerializeField] [TextArea] public string typeDescription;
        [SerializeField] public WeightedLocationFeature originLocationFeatures;
        [SerializeField] public WeightedLocationFeature endLocationFeatures;
        [SerializeField] public WeightedLocationFeature weightedLocationFeatures;
        [SerializeField] public WeightedLocationFeature weightedSecondaryLocationFeatures;
        [SerializeField] public int maxSecondaryFeatures = 3;
        [SerializeField] public int minSecondaryFeatures = 1;
        [SerializeField] public int maxShops = 2;
        [SerializeField] public int minShops = 2;
        [SerializeField] public int minEventNodes = 1;
        [SerializeField] public int maxEventNodes = 3;
        [SerializeField] public int minLocations = 9;
        [SerializeField] public int maxLocations = 12;
    }
}