namespace Building.Data
{
    public readonly struct BuildingFootprint
    {
        public readonly int Width;
        public readonly int Height;

        public BuildingFootprint(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}