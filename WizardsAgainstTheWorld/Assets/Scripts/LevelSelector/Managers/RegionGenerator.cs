using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Items;
using LevelSelector;
using LevelSelector.GameEvents;
using LevelSelector.Managers;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers.LevelSelector
{
    public interface IRegionGenerator
    {
        RegionData Generate(float difficulty, RegionType regionType = null);
        void SetSeed(int seed);
        RegionType GetRegionType(string typeName);
    }

    public class RegionGenerator : MonoBehaviour, IRegionGenerator
    {
        [Inject] private ILocationGenerator _locationGenerator;
        [Inject] private IShopGenerator _shopGenerator;

        [SerializeField] private int minJumps = 3;
        [SerializeField] private float maxJumpDistance = 0.4f;
        [SerializeField] private float minDistance = 0.2f;
        [SerializeField] private float endGameDifficulty = 5f;
        [SerializeField] private LocationFeature endBossFeature;

        [SerializeField] private List<ItemBehaviour> startLocationShopItems;
        [SerializeField] private Trader startLocationShopTrader;

        private System.Random _random;
        private RegionType[] _regionTypes;

        private void Awake()
        {
            _regionTypes = Resources.LoadAll<RegionType>("RegionTypes");
        }

        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        public RegionType GetRegionType(string typeName)
        {
            return _regionTypes.FirstOrDefault(rt => rt.typeName == typeName);
        }

        public RegionData Generate(float difficulty, RegionType regionType = null)
        {
            if (_random == null)
            {
                GameLogger.LogWarning("Seed was not provided, getting a random one instead");
                SetSeed(Guid.NewGuid().GetHashCode());
            }

            if (regionType == null)
            {
                regionType = PickRegionType(difficulty);
            }
            else
            {
                GameLogger.LogWarning($"Region type was overridden with {regionType.typeName}");
            }

            var region = GenerateLevels(minJumps, maxJumpDistance, minDistance, regionType, difficulty);

            if (region.Difficulity == 1)
            {
                var startLocation = region.Locations.Single(l => l.Type == LocationType.StartNode);
                startLocation.ShopData = new ShopData()
                {
                    traderIdentifier = startLocationShopTrader.GetIdentifier(),
                    fuelPriceMultiplier = 1f,
                    juicePriceMultiplier = 1f,
                    priceMultiplier = 1f,
                    itemCount = startLocationShopItems.Count,
                    inventory = new InventoryData()
                    {
                        Items = startLocationShopItems.Select(ItemData.FromItem).ToList()
                    }
                };
            }

            if (difficulty >= endGameDifficulty)
            {
                MakeRegionEndGame(region, regionType);
            }

            return region;
        }

        private RegionType PickRegionType(float difficulty)
        {
            return _regionTypes
                .Where(type => type.maxDifficulty >= difficulty && type.minDifficulty <= difficulty)
                .RandomElement(_random);
        }

        private RegionData GenerateLevels(int minJumps, float maxJumpDistance, float minDistance, RegionType regionType, float difficulty)
        {
            var region = new RegionData { Name = Names.Regions.RandomElement(_random) };
            var locations = GenerateLocations(_random.Next(regionType.minLocations, regionType.maxLocations + 1), minDistance, difficulty);

            if (locations.Count == 0)
                return region;

            AssignNodeTypes(locations);
            GenerateConnections(locations, maxJumpDistance);
            EnsureGraphConnectivity(locations);
            AddFeatures(locations, regionType);
            AddBossNodeFeature(locations, regionType);
            GenerateShops(locations, _random.Next(regionType.minShops, regionType.maxShops + 1));
            AddEventNodes(locations, _random.Next(regionType.minEventNodes, regionType.maxEventNodes + 1));
            RemoveFeaturesFromStartShopEndAndEventNodes(locations);

            locations.ForEach(region.AddLocation);
            region.Type = regionType.typeName;
            GameLogger.Log($"Generated {region.Locations.Count} levels with minDistance: {minDistance}.");

            region.RecalculateNeighbours();
            return region;
        }

        private void AddEventNodes(List<LocationData> locations, int nodesCount)
        {
            if (nodesCount <= 0) return;

            var eventLocations = locations
                .Where(l => l.Type == LocationType.Default)
                .OrderBy(_ => _random.Next())
                .Take(nodesCount)
                .ToList();

            foreach (var location in eventLocations)
            {
                location.Type = LocationType.EventNode;
                location.EventId = GameEventRegistry.GetAll().RandomElement(_random).Id;
                GameLogger.Log($"Added event node with ID: {location.EventId} at location: {location.Name}");
            }
        }

        private void RemoveFeaturesFromStartShopEndAndEventNodes(List<LocationData> locations)
        {
            locations.Single(l => l.Type == LocationType.EndNode).Features.Clear();
            foreach (var location in locations.Where(l => l.Type is LocationType.ShopNode or LocationType.EventNode or LocationType.StartNode))
            {
                location.Features.Clear();
            }
        }

        private void AddBossNodeFeature(List<LocationData> locations, RegionType regionType)
        {
            var endNode = locations.Single(l => l.Type == LocationType.BossNode);

            if (regionType.endLocationFeatures.Count == 0)
            {
                endNode.Type = LocationType.Default;
                GameLogger.LogWarning("No end location features defined for this region type, skipping feature addition.");
                return;
            }

            var endFeature = regionType.endLocationFeatures.GetRandomItem(_random).ToData();
            
            if(endFeature.fullLocationOverride)
            {
                endNode.Features.Clear();
            }
            
            endNode.Features.Add(endFeature);
        }

        private void MakeRegionEndGame(RegionData regionData, RegionType regionType)
        {
            // Remove the mini-boss node
            var miniBossNode = regionData.Locations.Single(l => l.Type == LocationType.BossNode);
            miniBossNode.Type = LocationType.Default;
            var bossFeatures = regionType.endLocationFeatures.GetValues();
            miniBossNode.Features
                .Where(x => bossFeatures.Any(y => y.ToData().Name == x.Name))
                .ToList()
                .ForEach(x => miniBossNode.Features.Remove(x));

            // Make the exit node the END BOSS node
            var exitNode = regionData.Locations.Single(l => l.Type == LocationType.EndNode);
            exitNode.Type = LocationType.BossNode;
            exitNode.Features.Clear();
            exitNode.Features.Add(endBossFeature.ToData());

            // 
            regionData.AdditionalComments.Add("<color=red>This region contains F.M.S. (Federation's Main Station), " +
                                              "this is where all our preparations will prove their worth</color>");
        }

        private void AddFeatures(List<LocationData> locations, RegionType regionType)
        {
            foreach (var location in locations)
            {
                _locationGenerator.AddFeatures(location, regionType);
            }
        }

        private void GenerateShops(ICollection<LocationData> locations, int shopCount)
        {
            for (int i = 0; i < shopCount; i++)
            {
                var location = locations
                    .Where(
                        x =>
                            x.Type != LocationType.StartNode
                            && x.Type != LocationType.EndNode
                            && x.Type != LocationType.BossNode
                            && x.ShopData is null)
                    .Where(location => location.Neighbours.All(n => n.ShopData is null))
                    .RandomElementOrDefault(_random);

                if (location == null)
                {
                    GameLogger.LogWarning("No more locations to add shops to.");
                    break;
                }

                location.ShopData = _shopGenerator.GenerateShop();
                location.Type = LocationType.ShopNode;
            }
        }

        private List<LocationData> GenerateLocations(int count, float minDistance, float difficulty)
        {
            var locations = new List<LocationData>();
            var positions = new List<Vector2>();
            const int maxAttempts = 100;
            
            var locationRandom = new System.Random(_random.Next());

            for (int i = 0; i < count; i++)
            {
                Vector2 position = GenerateUniquePosition(
                    positions,
                    minDistance,
                    maxAttempts
                );
                if (position == Vector2.negativeInfinity)
                    continue;

                var location = _locationGenerator.GenerateLocation(locationRandom, difficulty);
                location.Position = position;
                locations.Add(location);
                positions.Add(position);
            }

            return locations;
        }

        private Vector2 GenerateUniquePosition(List<Vector2> positions, float minDistance, int maxAttempts)
        {
            int attempts = 0;
            Vector2 randomPosition;

            do
            {
                randomPosition = new Vector2(_random.Value(), _random.Value());
                attempts++;
            } while (positions.Any(pos => Vector2.Distance(pos, randomPosition) < minDistance) && attempts < maxAttempts);

            return attempts >= maxAttempts ? Vector2.negativeInfinity : randomPosition;
        }

        private void AssignNodeTypes(List<LocationData> locations)
        {
            var startLocation = locations[_random.Next(locations.Count)];
            var endLocation = locations
                .OrderByDescending(l => Vector2.Distance(startLocation.Position, l.Position))
                .First();
            var bossLocation = locations
                .OrderByDescending(l => Vector2.Distance(startLocation.Position, l.Position))
                .Skip(1)
                .First();

            startLocation.Type = LocationType.StartNode;
            endLocation.Type = LocationType.EndNode;
            bossLocation.Type = LocationType.BossNode;
        }

        private void GenerateConnections(List<LocationData> locations, float maxJumpDistance)
        {
            foreach (var location in locations)
            {
                var neighbours = locations
                    .Where(l => l != location && Vector2.Distance(location.Position, l.Position) <= maxJumpDistance)
                    .OrderBy(l => Vector2.Distance(location.Position, l.Position))
                    .Take(4)
                    .ToList();

                foreach (var neighbour in neighbours)
                {
                    AddMutualConnection(location, neighbour);
                }
            }
        }

        private void EnsureGraphConnectivity(List<LocationData> locations)
        {
            var visited = new HashSet<LocationData>();
            var components = new List<List<LocationData>>();

            foreach (var location in locations)
            {
                if (visited.Contains(location))
                    continue;

                var component = new List<LocationData>();
                var stack = new Stack<LocationData>();
                stack.Push(location);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (!visited.Add(current)) continue;

                    component.Add(current);
                    foreach (var neighbor in current.Neighbours)
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }

                components.Add(component);
            }

            var largestComponent = components.OrderByDescending(c => c.Count).FirstOrDefault();
            if (largestComponent == null) return;

            foreach (var component in components)
            {
                if (component == largestComponent) continue;

                var closestA = component.OrderBy(l => largestComponent.Min(l2 => Vector2.Distance(l.Position, l2.Position))).FirstOrDefault();
                var closestB = largestComponent.OrderBy(l => Vector2.Distance(closestA.Position, l.Position)).FirstOrDefault();

                if (closestA != null && closestB != null)
                {
                    AddMutualConnection(closestA, closestB);
                }
            }
        }

        private void AddMutualConnection(LocationData a, LocationData b)
        {
            if (!a.Neighbours.Contains(b))
            {
                a.Neighbours.Add(b);
                a.NeighbourIds = a.Neighbours.Select(l => l.Id).ToArray();
            }

            if (!b.Neighbours.Contains(a))
            {
                b.Neighbours.Add(a);
                b.NeighbourIds = b.Neighbours.Select(l => l.Id).ToArray();
            }
        }
    }
}
