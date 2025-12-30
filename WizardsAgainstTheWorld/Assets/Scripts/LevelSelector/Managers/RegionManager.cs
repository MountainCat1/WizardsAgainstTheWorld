using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Zenject;

namespace Managers.LevelSelector
{
    public interface IRegionManager
    {
        public event Action RegionChanged;
        public RegionData Region { get; }
        IEnumerable<RegionData> NextRegions { get; }
        public void SetRegion(RegionData region, bool invokeEvent = true);
        public int GetDistance(Guid fromGuid, Guid toGuid);
        public int GetDistance(LocationData from, LocationData to);
    }


    public class RegionManager : MonoBehaviour, IRegionManager
    {
        public event Action RegionChanged;

        [Inject] private IDataManager _dataManager;
        [Inject] private IRegionGenerator _regionGenerator;

        public RegionData Region { get; private set; }

        public IEnumerable<RegionData> NextRegions => _nextRegions;

        private List<RegionData> _nextRegions = new();

        public int GetDistance(Guid fromGuid, Guid toGuid)
        {
            return GetDistance(
                Region.Locations.First(l => l.Id == fromGuid),
                Region.Locations.First(l => l.Id == toGuid)
            );
        }

        public int GetDistance(LocationData from, LocationData to)
        {
            if (from == to)
                return 0;

            Queue<(LocationData location, int distance)> queue = new();
            HashSet<LocationData> visited = new();

            queue.Enqueue((from, 0));
            visited.Add(from);

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                foreach (var neighbour in current.Neighbours)
                {
                    if (!visited.Contains(neighbour))
                    {
                        if (neighbour == to)
                        {
                            return distance + 1; // Found shortest path
                        }

                        queue.Enqueue((neighbour, distance + 1));
                        visited.Add(neighbour);
                    }
                }
            }

            return -1; // No path found
        }

        public void SetRegion(RegionData region, bool invokeEvent = true)
        {
            Region = region;

            _nextRegions = new List<RegionData>()
            {
                _regionGenerator.Generate(difficulty: Region.Difficulity + 1),
                _regionGenerator.Generate(difficulty: Region.Difficulity + 1),
            };

            foreach (var nextRegion in _nextRegions)
            {
                nextRegion.Difficulity = Region.Difficulity + 1;
            }
            
            if (invokeEvent)
            {
                RegionChanged?.Invoke();
            }
        }
    }
}