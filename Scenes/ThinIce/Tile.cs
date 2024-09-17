using Godot;
using System;


namespace ClubPenguinPlus.ThinIce
{
    /// <summary>
    /// Object for a single tile in the Thin Ice game
    /// </summary>
    public partial class Tile : Sprite2D
    {
        public Game.TileType TileType { get; set; }

        /// <summary>
        /// Reference to a key object if this tile has one
        /// </summary>
        public Sprite2D KeyReference { get; set; }

        /// <summary>
        /// Reference to a block object if it's on this tile
        /// </summary>
        public Block BlockReference { get; set; }

        /// <summary>
        /// Reference to the teleporter this tile is linked to, if it is a teleporter
        /// </summary>
        public Tile LinkedTeleporter { get; set; }

        /// <summary>
        /// Whether or not this tile is a plaid teleporter
        /// </summary>
        public bool IsPlaidTeleporter { get; set; }

        /// <summary>
        /// Coordinate of the tile in the grid
        /// </summary>
        public Vector2I TileCoordinate { get; set; }

        /// <summary>
        /// Reference to the game
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Reference to the coin bag on this tile, or null if none exists
        /// </summary>
        public Sprite2D CoinBag { get; set; } = null;

        /// <summary>
        /// Name of all the animations in sprite frames
        /// </summary>
        public static readonly StringName Animation = new("default");

        public FramerateBoundAnimation WaterAnimation { get; set; }

        public FramerateBoundAnimation TeleporterIdleAnimation { get; set; }

        public override void _Ready()
        {
            WaterAnimation = new(Game.WaterTileFrames, this);
            TeleporterIdleAnimation = new(Game.TeleporterTileFrames, this);
        }

        public override void _Process(double delta)
        {
            // water tile animation
            if (TileType == Game.TileType.Water)
            {
                WaterAnimation.Advance();
            }
            else if (TileType == Game.TileType.Teleporter && !IsPlaidTeleporter)
            {
                TeleporterIdleAnimation.Advance();
            }
        }

        /// <summary>
        /// Change the tile to a fresh new one of a given type
        /// </summary>
        /// <param name="tileType"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ChangeTile(Game.TileType tileType)
        {
            RemoveKey();
            RemoveCoinBag();
            LinkedTeleporter = null;
            BlockReference = null;
            IsPlaidTeleporter = false;

            if (tileType == Game.TileType.Water)
            {
                WaterAnimation.Start();
            }
            else if (tileType == Game.TileType.Teleporter)
            {
                TeleporterIdleAnimation.Start();
            }
            else
            {
                Texture2D tileTexture = tileType switch
                {
                    Game.TileType.Empty => Game.EmptyTile,
                    Game.TileType.Ice => Game.IceTile,
                    Game.TileType.ThickIce => Game.ThickIceTile,
                    Game.TileType.Wall => Game.WallTile,
                    Game.TileType.Goal => Game.GoalTile,
                    Game.TileType.PlaidTeleporter => Game.PlaidTeleporterTile,
                    Game.TileType.Lock => Game.LockTile,
                    Game.TileType.Button => Game.ButtonTile,
                    Game.TileType.FakeTemporaryWall => Game.WallTile,
                    Game.TileType.FakeImpassableWall => Game.WallTile,
                    Game.TileType.FakePassableWall => Game.WallTile,
                    Game.TileType.BlockHole => Game.BlockHoleTile,
                    _ => throw new NotImplementedException(),
                };

                Texture = tileTexture;
            }
            TileType = tileType;
        }

        /// <summary>
        /// Action to perform when the puffle enters this tile
        /// </summary>
        /// <param name="direction"></param>
        public void OnPuffleEnter(Puffle.Direction direction)
        {
            if (CoinBag != null)
            {
                GetCoinBag();
            }
            if (TileType == Game.TileType.Goal)
            {
                Game.GoToNextLevel();
            }
            // we must have the key already
            // also, this functionality should be changed to work on adjacent tile enter
            else if (TileType == Game.TileType.Lock)
            {
                ChangeTile(Game.TileType.Ice);
                Game.Puffle.UseKey();
            }
            else if (!IsPlaidTeleporter && TileType == Game.TileType.Teleporter)
            {
                Game.Puffle.TeleportTo(LinkedTeleporter.TileCoordinate);
                MakePlaidTeleporter();
                LinkedTeleporter.MakePlaidTeleporter();
            }
            else if (Game.Puffle.IsStuck())
            {
                Game.ResetLevel();
            }

            if (KeyReference != null)
            {
                RemoveKey();
                Game.Puffle.GetKey();
            }
        }

        /// <summary>
        /// Action to perform when the puffle exits this tile
        /// </summary>
        public void OnPuffleExit(Puffle.Direction direction)
        {
            if (TileType == Game.TileType.Ice)
            {
                Game.MeltTile();
                ChangeTile(Game.TileType.Water);
            }
            else if (TileType == Game.TileType.ThickIce)
            {
                Game.MeltTile();
                ChangeTile(Game.TileType.Ice);
            }

            Tile destinationTile = GetAdjacent(direction);

            // move if block exists
            destinationTile.BlockReference?.Move(direction);

            // button is pressed on previous tile exit
            if (destinationTile.TileType == Game.TileType.Button)
            {
                Game.PressButton();
            }
        }

        /// <summary>
        /// Make this teleporter a plaid teleporter
        /// </summary>
        public void MakePlaidTeleporter()
        {
            IsPlaidTeleporter = true;
            Texture = Game.PlaidTeleporterTile;
        }

        /// <summary>
        /// Remove key from tile
        /// </summary>
        public void RemoveKey()
        {
            if (KeyReference != null)
            {
                KeyReference.QueueFree();
                KeyReference = null;
            }
        }

        /// <summary>
        /// Add key to tile
        /// </summary>
        public void AddKey()
        {
            KeyReference = new Sprite2D();
            KeyReference.Texture = Game.KeyTexture;
            AddChild(KeyReference);
        }

        /// <summary>
        /// Get adjacent tile in a given direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Tile GetAdjacent(Puffle.Direction direction)
        {
            return Game.GetTile(Puffle.GetDestination(TileCoordinate, direction));
        }

        public void RemoveCoinBag()
        {
            if (CoinBag != null)
            {
                CoinBag.QueueFree();
                CoinBag = null;
            }
        }

        /// <summary>
        /// Removes and returns the value of the coin bag
        /// </summary>
        public void GetCoinBag()
        {
            Game.PointsInLevel += 100;
            RemoveCoinBag();
        }
    }
}
