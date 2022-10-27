namespace Dungeon.Generator
{
    /// <summary>
    /// Preset dungeon map sizes
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum TileMapSize
    {
        /// <summary>
        /// Generates a 5 x 5 cell map
        /// </summary>
        Small = 5,
        /// <summary>
        /// Generates a 11 x 11 cell map
        /// </summary>
        Medium = 11,
        /// <summary>
        /// Generates a 17 x 17 cell map
        /// </summary>
        Large = 17,
        /// <summary>
        /// Generates a 23 x 23 cell map
        /// </summary>
        Huge = 23,
        /// <summary>
        /// Generates a 31 x 31 cell map
        /// </summary>
        Gargantuan = 31,
        /// <summary>
        /// Generates a 63 x 63 cell map
        /// </summary>
        Biblical = 63
    }
}