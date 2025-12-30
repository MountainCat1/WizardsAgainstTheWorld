using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Services.MapGenerators;
using UnityEngine;
using Utilities;
using Zenject;

public interface IRoomDecorator
{
    void DecorateRooms(ICollection<RoomData> roomData, float tileSize);
    void RemoveProps();
}

public partial class RoomDecorator : MonoBehaviour, IRoomDecorator
{
    [Inject] private DiContainer _context = null!;

    [SerializeField] private GameObject roomMarker;

    [SerializeField] private RoomBlueprint startingRoomBlueprint;
    [SerializeField] private Transform propsParent;

    private List<Vector2Int> _occupiedPositions = new();

    private RoomData _entranceRoom = null!;

    public void DecorateRooms(ICollection<RoomData> roomData, float tileSize)
    {
        var roomQueue = new Queue<RoomData>(roomData.OrderBy(r => r.RoomID));

        if (!GameManager.GameSetup?.Location?.Features?.Any() ?? true)
        {
            GameLogger.LogError(
                "RoomDecorator: GameSetup or Location or Features is not set. Cannot decorate rooms."
            );
            return;
        }
        
        var startingRoom = DecorateStartingRoom(roomQueue, tileSize);
        DecorateRoomsFromFeatures(roomQueue, startingRoom, tileSize);
        DecorateRemainingRoomsWithGenericBlueprints(roomQueue, tileSize);
    }

    public void RemoveProps()
    {
        if (propsParent == null)
        {
            GameLogger.LogError("Props parent is not set.");
            return;
        }

        foreach (Transform child in propsParent)
        {
            Destroy(child.gameObject);
        }

        _occupiedPositions.Clear();
        GameLogger.Log("All props removed.");
    }

    private RoomData DecorateStartingRoom(Queue<RoomData> roomQueue, float tileSize)
    {
        if (roomQueue.Count == 0)
        {
            throw new InvalidCastException("No rooms to decorate.");
        }

        var startingRoom = roomQueue.Dequeue();
        GameLogger.Log($"Decorating starting room {startingRoom.RoomID} with blueprint {startingRoomBlueprint.Name}");
        DecorateRoom(startingRoom, startingRoomBlueprint, tileSize);

        return startingRoom;
    }

    private void DecorateRoomsFromFeatures(Queue<RoomData> roomQueue, RoomData entranceRoom, float tileSize)
    {
        var features = GameManager.GameSetup.Location.Features;

        var room = roomQueue.ToArray();
        var distances = ComputeRoomDistances(room, entranceRoom);

        var bluePrints = features
            .SelectMany(x => x.RoomBlueprints)
            .Select(LoadRoomBlueprint)
            .OrderByDescending(x => x.FarAwayRoom || x.BossRoom)
            .ToArray();

        foreach (var roomBlueprint in bluePrints)
        {
            if (roomBlueprint == null)
                continue;

            if (distances == null)
            {
                GameLogger.LogWarning("Distances are null, cannot find farthest room.");
            }

            // If this is a boss room blueprint, pick farthest room
            if ((roomBlueprint.BossRoom || roomBlueprint.FarAwayRoom) && distances != null)
            {
                var farthestRoomID = distances
                    .Where(kv => roomQueue.Any(r => r.RoomID == kv.Key))
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => kv.Key)
                    .FirstOrDefault();

                var farRoom = roomQueue.FirstOrDefault(r => r.RoomID == farthestRoomID);
                if (farRoom != null)
                {
                    // A REALLY DISGUSTING HACK
                    // We need to do it, coz the only other way would be to do 
                    // roomQueue = new Queue<RoomData>(roomQueue.Where(r => r.RoomID != farthestRoomID));
                    // But then we would reassign the reference and the next decorating method would not see
                    // changes applied to the queue
                    var filtered = roomQueue.Where(r => r.RoomID != farthestRoomID).ToList();
                    roomQueue.Clear();
                    foreach (var r in filtered)
                        roomQueue.Enqueue(r);


                    DecorateRoom(farRoom, roomBlueprint, tileSize);
                    continue;
                }
            }

            DecorateRooms(roomQueue, roomBlueprint, tileSize);
        }
    }

    private void DecorateRemainingRoomsWithGenericBlueprints(
        Queue<RoomData> roomQueue,
        float tileSize
    )
    {
        var genericRooms = GameManager.GameSetup.Location.Features
            .SelectMany(x => x.GenericRoomBlueprints)
            .ToList();

        if (!genericRooms.Any())
        {
            GameLogger.LogError("No generic room blueprints found.");
            return;
        }

        while (roomQueue.Any())
        {
            var blueprintName = genericRooms.RandomElement();
            var roomBlueprint = LoadRoomBlueprint(blueprintName);
            if (roomBlueprint == null)
                continue;

            DecorateRooms(roomQueue, roomBlueprint, tileSize);
        }
    }

    private void DecorateRooms(
        Queue<RoomData> roomQueue,
        RoomBlueprint roomBlueprint,
        float tileSize
    )
    {
        if (roomQueue.Count == 0)
        {
            GameLogger.LogError(
                "Not enough rooms to decorate all blueprints. Decorated rooms with this blueprint."
            );
        }

        DecorateRoom(roomQueue.Dequeue(), roomBlueprint, tileSize);
    }

    private RoomBlueprint LoadRoomBlueprint(string blueprintName)
    {
        var roomBlueprint = Resources.Load($"Rooms/{blueprintName}") as RoomBlueprint;

        if (roomBlueprint == null)
        {
            GameLogger.LogError($"Room blueprint {blueprintName} not found");
        }

        return roomBlueprint;
    }

    private void DecorateRoom(RoomData roomData, RoomBlueprint blueprint, float tileSize)
    {
        GameLogger.Log($"Decorating room {roomData.RoomID} with blueprint {blueprint.Name}");

        PlaceRoomMarker(roomData, blueprint, tileSize);

        roomData.OccupiedPositions = SpawnProps(roomData, blueprint, tileSize);

        var enemies = new List<Creature>();

        foreach (var roomEnemy in blueprint.Enemies)
        {
            for (int i = 0; i < UnityEngine.Random.Range(roomEnemy.minAmount, roomEnemy.maxAmount); i++)
            {
                enemies.Add(roomEnemy.enemy);
            }
        }

        roomData.Enemies = enemies.ToArray();
        roomData.IsEntrance = blueprint.StartingRoom;
        roomData.IsBoss = blueprint.BossRoom;
    }

    private void PlaceRoomMarker(
        RoomData roomData,
        RoomBlueprint blueprint,
        float tileSize
    )
    {
        var roomCenter = GetRoomCenter(roomData, tileSize);
        var marker = Instantiate(roomMarker, roomCenter, Quaternion.identity, propsParent);
        marker.name = $"{blueprint.name} ({roomData.RoomID})";
    }

    private Vector2 GetRoomCenter(RoomData roomData, float tileSize)
    {
        var roomCenterX = (float)roomData.Positions.Average(i => i.x);
        var roomCenterY = (float)roomData.Positions.Average(i => i.y);
        return new Vector2(roomCenterX, roomCenterY) * tileSize;
    }

    private List<Vector2Int> SpawnProps(
        RoomData roomData,
        RoomBlueprint blueprint,
        float tileSize)
    {
        var propPositions = new List<Vector2Int>();

        foreach (var prop in blueprint.Props)
        {
            for (int i = 0; i < prop.count; i++)
            {
                Vector2Int? randomPosition = null;

                switch (prop.position)
                {
                    case PropPosition.Anywhere:
                        randomPosition = GetRandomAvailablePosition(roomData);
                        break;
                    case PropPosition.Center:
                        randomPosition = GetCenterPosition(roomData);
                        break;
                    case PropPosition.Corner:
                        randomPosition = GetCornerPosition(roomData);
                        break;
                    case PropPosition.NotEdge:
                        randomPosition = GetNotEdgePosition(roomData);
                        break;
                    case PropPosition.Edge:
                        randomPosition = GetEdgePosition(roomData);
                        break;
                }

                if (randomPosition != null)
                {
                    var spawnPosition = (Vector2)randomPosition * tileSize +
                                        new Vector2(tileSize, tileSize) / 2 + prop.offset;
                    InstantiatePrefab(prop.prefab, spawnPosition);

                    _occupiedPositions.Add(randomPosition.Value);
                    propPositions.Add(randomPosition.Value);
                }
                else if (prop.required)
                {
                    throw new InvalidOperationException(
                        "No available position for prop {prop.prefab.name} in room {roomData.RoomID}. Required prop cannot be placed.");
                }
            }
        }

        return propPositions;
    }

    private Vector2Int? GetRandomAvailablePosition(RoomData roomData)
    {
        var availablePositions = roomData.Positions.Where(ValidatePosition).ToList();
        return availablePositions.Any() ? availablePositions.RandomElement() : null;
    }

    private Vector2Int? GetCenterPosition(RoomData roomData)
    {
        var minX = roomData.Positions.Min(p => p.x);
        var maxX = roomData.Positions.Max(p => p.x);
        var minY = roomData.Positions.Min(p => p.y);
        var maxY = roomData.Positions.Max(p => p.y);

        var centerPositions = roomData.Positions.Where(p =>
            p.x > minX && p.x < maxX && p.y > minY && p.y < maxY && ValidatePosition(p)
        ).ToList();

        return centerPositions.Any() ? centerPositions.RandomElement() : null;
    }


    private Vector2Int? GetCornerPosition(RoomData roomData)
    {
        var minX = roomData.Positions.Min(p => p.x);
        var maxX = roomData.Positions.Max(p => p.x);
        var minY = roomData.Positions.Min(p => p.y);
        var maxY = roomData.Positions.Max(p => p.y);

        var cornerPositions = new List<Vector2Int>
        {
            new(minX, minY),
            new(minX, maxY),
            new(maxX, minY),
            new(maxX, maxY)
        }.Where(ValidatePosition).ToList();

        return cornerPositions.Any() ? cornerPositions.RandomElement() : null;
    }

    private Vector2Int? GetEdgePosition(RoomData roomData)
    {
        var minX = roomData.Positions.Min(p => p.x);
        var maxX = roomData.Positions.Max(p => p.x);
        var minY = roomData.Positions.Min(p => p.y);
        var maxY = roomData.Positions.Max(p => p.y);

        var edgePositions = roomData.Positions.Where(p =>
            (p.x == minX || p.x == maxX || p.y == minY || p.y == maxY) && ValidatePosition(p)
        ).ToList();

        return edgePositions.Any() ? edgePositions.RandomElement() : null;
    }

    private Vector2Int? GetNotEdgePosition(RoomData roomData)
    {
        var minX = roomData.Positions.Min(p => p.x);
        var maxX = roomData.Positions.Max(p => p.x);
        var minY = roomData.Positions.Min(p => p.y);
        var maxY = roomData.Positions.Max(p => p.y);

        var notEdgePositions = roomData.Positions.Where(p =>
            (p.x != minX && p.x != maxX && p.y != minY && p.y != maxY) && ValidatePosition(p)
        ).ToList();

        return notEdgePositions.Any() ? notEdgePositions.RandomElement() : null;
    }

    private bool ValidatePosition(Vector2Int position)
    {
        return !_occupiedPositions.Contains(position);
    }

    private void InstantiatePrefab(GameObject prefab, Vector2 position)
    {
        GameLogger.Log($"Instantiating prefab {prefab.name} at {position}");
        _context.InstantiatePrefab(prefab, position, Quaternion.identity, propsParent);
    }

    private Dictionary<int, int> ComputeRoomDistances(ICollection<RoomData> allRooms, RoomData entranceRoom)
    {
        var distances = new Dictionary<int, int>();

        var entrancePosition = entranceRoom.Positions.GetAverageCenter();
        
        foreach (var room in allRooms)
        {
            var roomPosition = room.Positions.GetAverageCenter();
            var distance = (int)Vector2.Distance(entrancePosition, roomPosition);
            distances.Add(room.RoomID, distance);
        }

        return distances;
    }
}