using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Building.Data;
using GameplayScene.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Building
{
    public sealed class BuildingPrefab : MonoBehaviour
    {
        [Header("Definition")]
        [field: SerializeField]
        private string BuildingId { get; set; }

        [field: SerializeField] private int Width { get; set; }
        [field: SerializeField] private int Height { get; set; }
        [field: SerializeField] private float BuildTime { get; set; }
        [field: SerializeField] private Sprite Icon { get; set; }
        [field: SerializeField] private string NameKey { get; set; }
        [field: SerializeField] public List<GameResourceData> Costs { get; set; }
        [field: SerializeField] public List<BuildingPrefab> AvailableUpgrades { get; set; }
        [field: SerializeField] public SpriteRenderer MainSpriteRenderer { get; private set; }
        public BuildingFootprint Footprint => new BuildingFootprint(Width, Height);
        public string Name => LocalizationHelper.L($"Game.Buildings.{NameKey}");
        public BuildingView View => GetComponent<BuildingView>();

        [Pure]
        public string GetCostText()
        {
            if (Costs == null || Costs.Count == 0)
                return "No Cost";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var cost in Costs)
            {
                sb.Append($"{cost.amount} {cost.type}\n");
            }

            return sb.ToString().TrimEnd('\n');
        }
    }
}