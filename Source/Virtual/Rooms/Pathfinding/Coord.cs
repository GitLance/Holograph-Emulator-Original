namespace Holo.Virtual.Rooms.Pathfinding
{
    /// <summary>
    /// Represents a X,Y coordinate.
    /// </summary>
    struct Coord
    {
        /// <summary>
        /// The X value of the coord.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y value of the coord.
        /// </summary>
        internal int Y;
        /// <summary>
        /// Initializes the coord.
        /// </summary>
        /// <param name="X">The X value of the coord.</param>
        /// <param name="Y">The Y value of the coord.</param>
        internal Coord(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
