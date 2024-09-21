using Godot;
using System;
using ClubPenguinPlus.Utils;


namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Object for a single tile in the Thin Ice game
	/// </summary>
	public partial class Tile : Sprite2D
	{
		[Export]
		private Texture2D EmptyTile { get; set; }

		[Export]
		private Texture2D IceTile { get; set; }

		[Export]
		private Texture2D ThickIceTile { get; set; }

		[Export]
		private Texture2D WallTile { get; set; }

		[Export]
		private Texture2D GoalTile { get; set; }

		[Export]
		private Texture2D PlaidTeleporterTile { get; set; }

		[Export]
		private Texture2D LockTile { get; set; }

		[Export]
		private Texture2D ButtonTile { get; set; }

		[Export]
		private Texture2D BlockHoleTile { get; set; }

		[Export]
		private SpriteFrames WaterTileFrames { get; set; }

		[Export]
		private SpriteFrames TeleporterTileFrames { get; set; }

		[Export]
		private PackedScene KeyScene { get; set; }

		/// <summary>
		/// All valid tile types
		/// </summary>
		public enum Type
		{
			/// <summary>
			/// In vanilla, represents blank tiles placed outside the level
			/// </summary>
			Empty,

			/// <summary>
			/// Regular ice tiles
			/// </summary>
			Ice,

			/// <summary>
			/// Tiles after ice is melt
			/// </summary>
			Water,

			/// <summary>
			/// Tiles that can be passed through twice
			/// </summary>
			ThickIce,

			/// <summary>
			/// Regular wall tile
			/// </summary>
			Wall,

			/// <summary>
			/// Tile that finishes the level upon reached
			/// </summary>
			Goal,

			/// <summary>
			/// Tile that links to another and teleports player to it
			/// </summary>
			Teleporter,

			/// <summary>
			/// Tile for an already used teleporter
			/// </summary>
			PlaidTeleporter,

			/// <summary>
			/// Tile that requires a key to be opened
			/// </summary>
			Lock,

			/// <summary>
			/// Tile for the button that in vanilla is used to reveal the hidden path in
			/// level 19
			/// </summary>
			Button,

			/// <summary>
			/// Tile for the fake wall that in vanilla is used to hide the hidden path in
			/// level 19, but you can't walk over
			/// </summary>
			FakeImpassableWall,

			/// <summary>
			/// Tile for the fake wall that in vanilla is used to hide the hidden path in
			/// level 19 and that you can walk over
			/// </summary>
			FakePassableWall,

			/// <summary>
			/// Tile for the fake wall in vanilla that is used to hide the hidden path in
			/// level 19 and that becomes a normal wall when the button is pressed
			/// </summary>
			FakeTemporaryWall,

			/// <summary>
			/// Tile the blocks are meant to be placed into
			/// </summary>
			BlockHole
		}

		public Type TileType { get; private set; }

		/// <summary>
		/// Reference to a key object if this tile has one
		/// </summary>
		private Key KeyReference { get; set; }

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

		private FramerateBoundAnimation WaterAnimation { get; set; }

		private FramerateBoundAnimation TeleporterIdleAnimation { get; set; }

		// reference hardcoded values
		private static readonly int LeftmostTileX = -2158;
		private static readonly int TopmostTileY = -1700;

		public override void _Ready()
		{
			Texture = EmptyTile;
			WaterAnimation = new(WaterTileFrames, this);
			TeleporterIdleAnimation = new(TeleporterTileFrames, this);
		}

		public void SetCoordinate(int x, int y)
		{
			var tileSize = EmptyTile.GetSize();
			Position = new Vector2(LeftmostTileX + x * tileSize.X, TopmostTileY + y * tileSize.Y);
			TileCoordinate = new Vector2I(x, y);
		}

		public override void _Process(double delta)
		{
			// water tile animation
			if (TileType == Type.Water)
			{
				WaterAnimation.Advance();
			}
			else if (TileType == Type.Teleporter && !IsPlaidTeleporter)
			{
				TeleporterIdleAnimation.Advance();
			}
		}

		/// <summary>
		/// Change the tile to a fresh new one of a given type
		/// </summary>
		/// <param name="tileType"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void ChangeTile(Type tileType)
		{
			RemoveKey();
			RemoveCoinBag();
			LinkedTeleporter = null;
			BlockReference = null;
			IsPlaidTeleporter = false;

			if (tileType == Type.Water)
			{
				WaterAnimation.Start();
			}
			else if (tileType == Type.Teleporter)
			{
				TeleporterIdleAnimation.Start();
			}
			else
			{
				Texture2D tileTexture = tileType switch
				{
					Type.Empty => EmptyTile,
					Type.Ice => IceTile,
					Type.ThickIce => ThickIceTile,
					Type.Wall => WallTile,
					Type.Goal => GoalTile,
					Type.PlaidTeleporter => PlaidTeleporterTile,
					Type.Lock => LockTile,
					Type.Button => ButtonTile,
					Type.FakeTemporaryWall => WallTile,
					Type.FakeImpassableWall => WallTile,
					Type.FakePassableWall => WallTile,
					Type.BlockHole => BlockHoleTile,
					_ => throw new NotImplementedException(),
				};

				Texture = tileTexture;
			}
			TileType = tileType;
		}

		public void OnPuffleStartEnter(Direction direction)
		{
			BlockReference?.Move(direction);

			// button is pressed on previous tile exit
			if (TileType == Type.Button)
			{
				Game.PressButton();
			}
		}

		/// <summary>
		/// Action to perform when the puffle enters this tile
		/// </summary>
		/// <param name="direction"></param>
		public void OnPuffleFinishEnter(Puffle puffle)
		{
			if (CoinBag != null)
			{
				GetCoinBag();
			}
			if (TileType == Type.Goal)
			{
				Game.GoToNextLevel();
			}
			// we must have the key already
			// also, this functionality should be changed to work on adjacent tile enter
			else if (TileType == Type.Lock)
			{
				ChangeTile(Type.Ice);
				puffle.UseKey();
			}
			else if (!IsPlaidTeleporter && TileType == Type.Teleporter)
			{
				puffle.TeleportTo(LinkedTeleporter.TileCoordinate);
				MakePlaidTeleporter();
				LinkedTeleporter.MakePlaidTeleporter();
			}
			else if (puffle.IsStuck())
			{
				Game.ResetLevel();
			}

			if (KeyReference != null)
			{
				RemoveKey();
				puffle.GetKey();
			}
		}

		/// <summary>
		/// Action to perform when the puffle exits this tile
		/// </summary>
		public void OnPuffleExit()
		{
			if (TileType == Type.Ice || TileType == Type.ThickIce)
			{
				var newType = TileType == Type.Ice ? Type.Water : Type.Ice;
				Game.MeltTile();
				ChangeTile(newType);
			}
		}

		/// <summary>
		/// Make this teleporter a plaid teleporter
		/// </summary>
		public void MakePlaidTeleporter()
		{
			IsPlaidTeleporter = true;
			Texture = PlaidTeleporterTile;
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
			KeyReference = KeyScene.Instantiate<Key>();
			AddChild(KeyReference);
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
			Game.GetCoinBag();
			RemoveCoinBag();
		}
	}
}
