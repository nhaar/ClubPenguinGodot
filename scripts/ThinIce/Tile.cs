using Godot;
using System;


namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Object for a single tile in the Thin Ice game
	/// </summary>
	public partial class Tile : Sprite2D
	{
		[Export]
		public Texture2D EmptyTile { get; set; }

		[Export]
		public Texture2D IceTile { get; set; }

		[Export]
		public Texture2D ThickIceTile { get; set; }

		[Export]
		public Texture2D WallTile { get; set; }

		[Export]
		public Texture2D GoalTile { get; set; }

		[Export]
		public Texture2D PlaidTeleporterTile { get; set; }

		[Export]
		public Texture2D LockTile { get; set; }

		[Export]
		public Texture2D ButtonTile { get; set; }

		[Export]
		public Texture2D BlockHoleTile { get; set; }

		[Export]
		public SpriteFrames WaterTileFrames { get; set; }

		[Export]
		public SpriteFrames TeleporterTileFrames { get; set; }

		public Game.TileType TileType { get; set; }

		/// <summary>
		/// Reference to a key object if this tile has one
		/// </summary>
		public Key KeyReference { get; set; }

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
			WaterAnimation = new(WaterTileFrames, this);
			TeleporterIdleAnimation = new(TeleporterTileFrames, this);
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
					Game.TileType.Empty => EmptyTile,
					Game.TileType.Ice => IceTile,
					Game.TileType.ThickIce => ThickIceTile,
					Game.TileType.Wall => WallTile,
					Game.TileType.Goal => GoalTile,
					Game.TileType.PlaidTeleporter => PlaidTeleporterTile,
					Game.TileType.Lock => LockTile,
					Game.TileType.Button => ButtonTile,
					Game.TileType.FakeTemporaryWall => WallTile,
					Game.TileType.FakeImpassableWall => WallTile,
					Game.TileType.FakePassableWall => WallTile,
					Game.TileType.BlockHole => BlockHoleTile,
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

			var key = GD.Load<PackedScene>("res://scenes/thin_ice/key.tscn").Instantiate<Key>();
			KeyReference = key;
			AddChild(key);
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
