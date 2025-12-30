using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class CreateRoomsGenerationStep : GenerationStep
    {
        public override void Generate(GenerateMapData data, GenerateMapSettings settings, Random random)
        {
            var gridSize = data.GridSize;
            var roomCount = settings.roomCount;
            var roomMinSize = settings.roomMinSize;
            var roomMaxSize = settings.roomMaxSize;
            
            void SetFloor(int x, int y)
            {
                data.SetTile(x, y, TileType.Floor);
            }

            var rooms = GenerateRooms(SetFloor, gridSize, roomCount, roomMinSize, roomMaxSize, random);
            
            data.Rooms = rooms;
        }
        
        private List<RoomData> GenerateRooms(
            Action<int, int> setFloor, 
            Vector2Int gridSize,
            int roomCount, 
            Vector2Int roomMinSize, 
            Vector2Int roomMaxSize,
            Random random)
        {
            var rooms = new List<RoomData>();

            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = random.Next(roomMinSize.x, roomMaxSize.x + 1);
                int roomHeight = random.Next(roomMinSize.y, roomMaxSize.y + 1);

                int x = random.Next(1, gridSize.x - roomWidth - 1);
                int y = random.Next(1, gridSize.y - roomHeight - 1);

                var roomData = new RoomData { RoomID = i };

                for (int roomx = x; roomx < x + roomWidth; roomx++)
                {
                    for (int roomy = y; roomy < y + roomHeight; roomy++)
                    {
                        setFloor(roomx, roomy);
                        roomData.Positions.Add(new Vector2Int(roomx, roomy));
                    }
                }

                rooms.Add(roomData);
                GameLogger.Log($"Room {i} created at: {x}, {y}");
            }

            return rooms;
        }

    }
}