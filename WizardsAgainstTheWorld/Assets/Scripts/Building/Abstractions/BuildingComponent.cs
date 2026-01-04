using Components.Entities;
using UnityEngine;

namespace Building.Abstractions
{
    [RequireComponent(typeof(BuildingView))]
    public class BuildingComponent : EntityComponent
    {
        public BuildingView Building { get; set; }
        
        protected override void Awake()
        {
            base.Awake();
            
            Building = GetComponent<BuildingView>();
        }
    }
}