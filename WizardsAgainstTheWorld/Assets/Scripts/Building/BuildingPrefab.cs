using Building.Data;
using UnityEngine;

namespace Building
{
    public sealed class BuildingPrefab : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] private string buildingId;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float buildTime;
        [SerializeField] private Sprite icon;
        [SerializeField] private string nameKey;
        public BuildingFootprint Footprint => new BuildingFootprint(width, height);
        public string Name => nameKey;
    }
}