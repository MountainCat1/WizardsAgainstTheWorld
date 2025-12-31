using UnityEngine;
using Building.Data;

namespace Building
{
    public sealed class BuildingView : MonoBehaviour
    {
        [SerializeField] private int width = 1;
        [SerializeField] private int height = 1;

        public BuildingFootprint Footprint => new(width, height);
    }
}