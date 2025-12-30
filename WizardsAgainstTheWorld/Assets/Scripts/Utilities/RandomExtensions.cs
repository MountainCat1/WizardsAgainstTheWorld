namespace Utilities
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns random value from 0 to 1
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float Value(this System.Random random)
        {
            return (float) random.NextDouble();
        }
    }
}