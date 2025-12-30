using System;
using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class ConnectRoomsGenerationStep : GenerationStep
    {
        public override void Generate(
            GenerateMapData data,
            GenerateMapSettings settings,
            Random random
        )
        {
            var rooms = data.Rooms;

            void SetFloor(int x, int y)
            {
                data.SetTile(x, y, TileType.Floor);
            }

            for (int i = 0; i < rooms.Count - 1; i++)
            {
                var startRoom = rooms[i];
                var endRoom = rooms[i + 1];

                var start =
                    startRoom.Positions[random.Next(startRoom.Positions.Count)];
                var end = endRoom.Positions[random.Next(endRoom.Positions.Count)];

                CreateCorridor(SetFloor, start, end, (int)settings.corridorWidth);

                startRoom.ConnectedRoomIDs.Add(endRoom.RoomID);
                endRoom.ConnectedRoomIDs.Add(startRoom.RoomID);

                GameLogger.Log(
                    $"Corridor created between Room {startRoom.RoomID} and Room {endRoom.RoomID}"
                );
            }
        }

        private void CreateCorridor(
            Action<int, int> setFloor,
            Vector2Int start,
            Vector2Int end,
            int corridorWidth = 1
        )
        {
            var current = start;

            while (current != end)
            {
                if (current.x != end.x)
                {
                    current.x += Math.Sign(end.x - current.x);
                }
                else if (current.y != end.y)
                {
                    current.y += Math.Sign(end.y - current.y);
                }

                // Apply corridor width
                int halfWidth = corridorWidth / 2;
                int startOffset = -halfWidth;
                int endOffset = halfWidth;

                // Adjust offsets for even corridor widths
                if (corridorWidth % 2 == 0)
                {
                    startOffset = -halfWidth + 1;
                }

                for (int x = startOffset; x <= endOffset; x++)
                {
                    for (int y = startOffset; y <= endOffset; y++)
                    {
                        setFloor(current.x + x, current.y + y);
                    }
                }
            }

            GameLogger.Log($"Corridor created from: {start} to {end}");
        }
    }
}
