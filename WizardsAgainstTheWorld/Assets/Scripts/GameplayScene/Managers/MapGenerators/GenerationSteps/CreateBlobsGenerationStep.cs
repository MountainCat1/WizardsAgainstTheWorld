using System;
using UnityEngine;
using Random = System.Random;

namespace Services.MapGenerators.GenerationSteps
{
    public class CreateBlobsGenerationStep : GenerationStep
    {
        [SerializeField] private float blobDensity = 0.5f;
        [SerializeField] private int maxBlobSize = 20;
        [SerializeField] private int minBlobSize = 5;
        
        public override void Generate(
            GenerateMapData data,
            GenerateMapSettings settings,
            Random random
        )
        {
            // int blobCount = settings.blobCount;
            // int minBlobSize = settings.minBlobSize;
            // int maxBlobSize = settings.maxBlobSize;
            
            int blobCount = (int)(blobDensity * (data.GridSize.x * data.GridSize.y) / maxBlobSize);

            for (int i = 0; i < blobCount; i++)
            {
                var start = GetRandomValidPosition(data, random);
                int blobSize = random.Next(minBlobSize, maxBlobSize + 1);

                CreateBlob(
                    data,
                    start,
                    blobSize,
                    random
                );

                GameLogger.Log($"Blob {i} created at {start} with size {blobSize}");
            }
        }

        private void CreateBlob(
            GenerateMapData data,
            Vector2Int start,
            int size,
            Random random
        )
        {
            var current = start;
            data.SetTile(current.x, current.y, TileType.Wall);

            for (int i = 0; i < size; i++)
            {
                var direction = GetRandomDirection(random);
                var next = current + direction;

                if (!data.IsInBounds(next.x, next.y))
                    continue;

                data.SetTile(next.x, next.y, TileType.Wall);
                current = next;
            }
        }

        private Vector2Int GetRandomValidPosition(
            GenerateMapData data,
            Random random
        )
        {
            for (int i = 0; i < 1000; i++)
            {
                int x = random.Next(0, data.GridSize.x);
                int y = random.Next(0, data.GridSize.y);

                if (data.IsInBounds(x, y))
                    return new Vector2Int(x, y);
            }

            throw new InvalidOperationException(
                "Failed to find valid position for blob generation."
            );
        }

        private Vector2Int GetRandomDirection(Random random)
        {
            return random.Next(4) switch
            {
                0 => Vector2Int.up,
                1 => Vector2Int.down,
                2 => Vector2Int.left,
                3 => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }
    }
}
