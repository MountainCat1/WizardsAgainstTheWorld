using System.Collections.Generic;
using System.Linq;
using Services.MapGenerators;
using UnityEngine;
using Utilities;

namespace Managers.Helpers
{
    public class SpawnPositionSelector
    {
        private readonly MapData _mapData;
        private readonly HashSet<Vector2Int> _availablePositions;

        public SpawnPositionSelector(MapData mapData)
        {
            _mapData = mapData;
            var initiallyTakenPositions = mapData.GetAllRooms()
                .SelectMany(r => r.OccupiedPositions)
                .ToHashSet();

            _availablePositions = new HashSet<Vector2Int>(
                mapData.GetAllRooms().SelectMany(r => r.Positions)
                    .Where(p => !_mapData.GetAllRooms().SelectMany(r => r.OccupiedPositions).Contains(p))
                    .Except(initiallyTakenPositions)
                    .Where(p => mapData.GetTileType(p) == TileType.Floor)
            );
        }

        public Vector2Int? TakeRandomValidPosition(IEnumerable<Creature> avoidCreatures, float minDistance,
            float maxDistance)
        {
            int attemptsLeft = 50;

            while (attemptsLeft-- > 0 && _availablePositions.Any())
            {
                var pos = _availablePositions.RandomElementOrDefaultStruct();
                
                if (pos == null)
                    return null;

                var tooClose = avoidCreatures.Any(c => Vector2.Distance(c.transform.position, pos.Value) < minDistance);
                var tooFar = !avoidCreatures.Any(c => Vector2.Distance(c.transform.position, pos.Value) <= maxDistance);

                if (tooClose || tooFar)
                    continue;

                _availablePositions.Remove(pos.Value); // ensure it's not reused
                return pos;
            }

            return null;
        }

        public Vector2Int? TakeAny()
        {
            if (_availablePositions.Count == 0) return null;

            var pos = _availablePositions.RandomElementOrDefaultStruct();
            
            if (pos == null) return null;
            
            _availablePositions.Remove(pos.Value);
            return pos;
        }

        public void MarkAsTaken(Vector2Int position)
        {
            _availablePositions.Remove(position);
        }

        public Vector2Int? TakeExcludeNearPoints(IEnumerable<Vector2> points, float minDistance)
        {
            var filtered = _availablePositions.Where(p =>
                points.All(point => Vector2.Distance(p, point) > minDistance)).ToList();

            var randomPosition = filtered.RandomElementOrDefaultStruct();
            
            if (randomPosition == null)
                return null;

            _availablePositions.Remove(randomPosition.Value);

            return randomPosition;
        }

        public IEnumerable<Vector2Int> GetAvailablePositions() => _availablePositions.ToList();

        public Vector2Int? TakeRandomRoomPosition(RoomData room)
        {
            var floorPositions = room.Positions
                .Where(p => _mapData.GetTileType(p) == TileType.Floor)
                .Where(x => _availablePositions.Contains(x))
                .ToList();

            if (floorPositions.Count == 0)
                return null;

            var randomPosition = floorPositions.RandomElementOrDefaultStruct();
            
            if (randomPosition == null)
                return null;
            
            _availablePositions.Remove(randomPosition.Value);
            return randomPosition;
        }

        public Vector2Int? TakeExcludePositionsNearPoint(Vector3 exitPosition, float minDistance)
        {
            var filtered = _availablePositions
                .Where(p => Vector2.Distance(p, exitPosition) > minDistance)
                .ToList();

            if (!filtered.Any())
                return null;

            var randomPosition = filtered.RandomElementOrDefaultStruct();
            
            if (randomPosition == null)
                return null;
            
            _availablePositions.Remove(randomPosition.Value);
            return randomPosition;
        }

        public Vector2Int? TakeAnyOff(IEnumerable<Vector2Int> from)
        {
            var available = _availablePositions
                .Where(from.Contains)
                .ToList();

            if (available.Count == 0)
                return null;

            var randomPosition = available.RandomElementOrDefaultStruct();
            
            if (randomPosition == null)
                return null;
            
            _availablePositions.Remove(randomPosition.Value);
            return randomPosition;
        }

        public Vector2Int? GetRandomValidPosition(
            List<Vector2Int> positions,
            ICollection<Creature> playerCreatures,
            float minDistance,
            float maxDistanceFromPlayerCreatures)
        {
            int attemptsLeft = 50;

            while (attemptsLeft-- > 0 && positions.Any())
            {
                var pos = positions.RandomElementOrDefaultStruct();
                
                if (pos == null)
                    return null;

                var tooClose = playerCreatures.Any(c => Vector2.Distance(c.transform.position, pos.Value) < minDistance);
                var tooFar = !playerCreatures.Any(c => Vector2.Distance(c.transform.position, pos.Value) <= maxDistanceFromPlayerCreatures);

                if (tooClose || tooFar)
                    continue;

                // We don't remove coz its Get not Take
                
                return pos;
            }

            return null; 
        }
    }
}