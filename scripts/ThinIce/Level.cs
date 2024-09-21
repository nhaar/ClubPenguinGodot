using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
    /// <summary>
    /// Class for a Thin Ice level's layout
    /// </summary>
    internal class Level
    {
        public static readonly int MaxWidth = 19;

        public static readonly int MaxHeight = 15;

        /// <summary>
        /// Tiles that do not count towards the total tile count
        /// </summary>
        private static readonly List<Tile.Type> ZeroCountTiles = new()
        {
            Tile.Type.Empty,
            Tile.Type.Water,
            Tile.Type.Wall,
            Tile.Type.BlockHole,
            Tile.Type.Goal,
            Tile.Type.Teleporter,
            Tile.Type.FakeTemporaryWall,
            Tile.Type.Button
        };

        /// <summary>
        /// A coordinate pair used to offset the level's origin relative to the absolute one.
        /// When used, the top left square to the origin will be automatically
        /// filled with empty squares.
        /// </summary>
        public Vector2I RelativeOrigin { get; set; }

        // all coordinates used in here are in absolute coords relative to the whole grid,
        // and not the relative origin

        /// <summary>
        /// Coordinate pair for where the puffle should spawn
        /// </summary>
        public Vector2I PuffleSpawnLocation { get; set; }

        /// <summary>
        /// Biggest number of non empty horizontal tiles used relative to the origin
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Biggest number of non empty vertical tiles used relative to the origin
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Grid containing all tiles used in the level, not containing
        /// empty tiles outside the defined width, height and relative origin boundaries
        /// </summary>
        public Tile.Type[,] Tiles { get; set; }

        /// <summary>
        /// List of all cordinate pairs for where the keys should be placed
        /// </summary>
        public List<Vector2I> KeyPositions { get; set; }

        /// <summary>
        /// List of all coordinate pairs where blocks are spawned
        /// </summary>
        public List<Vector2I> BlockPositions { get; set; }

        /// <summary>
        /// Null if there is no coin bag, otherwise the coordinate pair for where the coin bag should be placed
        /// </summary>
        public Vector2I? CoinBagPosition { get; set; }

        /// <summary>
        /// Total tile count of the game (as per vanilla standards)
        /// </summary>
        public int TotalTileCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Tile.Type tileType = Tiles[i, j];
                        if (!ZeroCountTiles.Contains(tileType))
                        {
                            count++;
                            if (tileType == Tile.Type.ThickIce)
                            {
                                count++;
                            }
                        }
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Whether the given point is outside the level's defined boundaries
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsPointOutOfBounds(Vector2I point)
        {
            return point.X < RelativeOrigin.X || point.Y < RelativeOrigin.Y || point.X >= Width + RelativeOrigin.X || point.Y >= Height + RelativeOrigin.Y;
        }
    }

}
