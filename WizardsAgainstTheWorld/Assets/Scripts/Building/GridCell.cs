namespace Building
{
    public sealed class GridCell
    {
        public GridPosition Position { get; }
        public bool TerrainBlocked { get; private set; }
        public bool StructureBlocked { get; private set; }
        public int OccupyingUnits { get; private set; }

        public bool Walkable => !TerrainBlocked && !StructureBlocked;

        public GridCell(GridPosition position)
        {
            Position = position;
        }

        public void SetTerrainBlocked(bool value) => TerrainBlocked = value;
        public void SetStructureBlocked(bool value) => StructureBlocked = value;

        public void AddUnit() => OccupyingUnits++;
        public void RemoveUnit() => OccupyingUnits--;
    }

}