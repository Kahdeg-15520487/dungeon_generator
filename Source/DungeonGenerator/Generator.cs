namespace Dungeon.Generator
{
    /// <summary>
    /// Facade for generating dungeon like levels
    /// </summary>
    public static class CellBasedGeneratorCaller
    {
        public static CellBasedGenerator cbg;

        /// <summary>
        /// Generates a dungeon
        /// </summary>
        /// <param name="size">The desired size</param>
        /// <param name="parameters">Generation options</param>
        /// <returns>An <see cref="ITileMap"/>, a 2D array of tile information</returns>
        public static ITileMap Generate(int size, GeneratorParams parameters)
        {
            var map = new TileMap(size * CellBasedGenerator.CellSize, size * CellBasedGenerator.CellSize);
            cbg = new CellBasedGenerator(parameters);
            cbg.Generate(map);
            return map;
        }

        /// <summary>
        /// Generates a dungeon, and allows for an optional seed
        /// </summary>
        /// <param name="size">The desired size</param>
        /// <param name="seed">Generation seed</param>
        /// <returns>An <see cref="ITileMap"/>, a 2D array of tile information</returns>
        public static ITileMap Generate(int size, uint seed)
        {
            var map = new TileMap(size * CellBasedGenerator.CellSize, size * CellBasedGenerator.CellSize);

            var parameters = GeneratorParams.Default;
            parameters.Seed = seed;

            cbg = new CellBasedGenerator(parameters);
            cbg.Generate(map);

            return map;
        }

        /// <summary>
        /// Generates a dungeon
        /// </summary>
        /// <param name="width">The desired width</param>
        /// <param name="height">The desired height</param>
        /// <param name="seed">Generation seed</param>
        /// <returns>An <see cref="ITileMap"/>, a 2D array of tile information</returns>
        public static ITileMap Generate(int width, int height, uint seed)
        {
            var map = new TileMap(width * CellBasedGenerator.CellSize, height * CellBasedGenerator.CellSize);

            var parameters = GeneratorParams.Default;
            parameters.Seed = seed;

            new CellBasedGenerator(parameters).Generate(map);

            return map;
        }
    }
}