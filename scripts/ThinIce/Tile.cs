using Godot;
using System;
using ClubPenguinPlus.Utils;


namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Scene for a single tile in the Thin Ice game
	/// 
	/// By design, tiles are mutable, as opposed to handling them by deleting and creating new tiles
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
		private SpriteFrames TeleporterBootingAnimationFrames { get; set; }

		[Export]
		private SpriteFrames TeleporterTileFrames { get; set; }

		[Export]
		private SpriteFrames TeleporterChargeAnimationFrames { get; set; }

		[Export]
		private SpriteFrames MeltingAnimationFrames { get; set; }

		[Export]
		private SpriteFrames WhirlpoolAnimationFrames { get; set; }

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
			BlockHole,

			/// <summary>
			/// Water tile a puffle is sinking into
			/// </summary>
			Whirlpool
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
		public Engine Engine { get; set; }

		/// <summary>
		/// Reference to the coin bag on this tile, or null if none exists
		/// </summary>
		public Sprite2D CoinBag { get; set; } = null;

		private FramerateBoundAnimation WaterAnimation { get; set; }

		private FramerateBoundAnimation TeleporterBootingAnimation { get; set; }

		private FramerateBoundAnimation TeleporterIdleAnimation { get; set; }

		private FramerateBoundAnimation MeltingAnimation { get; set; }

		private FramerateBoundAnimation WhirlpoolAnimation { get; set; }

		private FramerateBoundAnimation TeleporterChargeAnimation { get; set; }

		private Sprite2D MeltingAnimationNode { get; set; } = null;

		private bool IsWhirlpool { get; set; } = false;

		private bool IsTeleporterBooting { get; set; } = false;

		/// <summary>
		/// Whether or not the teleporter is faster from being near a player
		/// </summary>
		public bool IsTeleporterCharge { get; private set; } = false;

		// reference hardcoded values
		private static readonly float LeftmostTileX = -4319.5f;
		private static readonly float TopmostTileY = -3638f;

		public override void _Ready()
		{
			Texture = EmptyTile;
			WaterAnimation = new(WaterTileFrames, this);
			TeleporterIdleAnimation = new(TeleporterTileFrames, this);
			TeleporterBootingAnimation = new(TeleporterBootingAnimationFrames, this);
			TeleporterChargeAnimation = new(TeleporterChargeAnimationFrames, this);
			WhirlpoolAnimation = new(WhirlpoolAnimationFrames, this);
		}

		private void StartMelt()
		{
			MeltingAnimationNode = new();
			AddChild(MeltingAnimationNode);
			MeltingAnimation = new(MeltingAnimationFrames, MeltingAnimationNode);
			MeltingAnimation.StartOnProcess();
		}

		private void ContinueMelt()
		{
			var ended = MeltingAnimation.Advance();
			if (ended)
			{
				MeltingAnimationNode.QueueFree();
				MeltingAnimationNode = null;
			}
		}

		/// <summary>
		/// Make a teleporter charged
		/// </summary>
		/// <param name="chargeOther">Whether or not should charge the linked one</param>
		public void Charge(bool chargeOther = true)
		{
			IsTeleporterCharge = true;
			TeleporterChargeAnimation.StartOnProcess();
			if (chargeOther)
			{
				LinkedTeleporter.Charge(false);
			}
		}

		/// <summary>
		/// Uncharge a teleporter
		/// </summary>
		/// <param name="unchargeOther">Whether or not should uncharge the linked one</param>
		public void Uncharge(bool unchargeOther = true)
		{
			IsTeleporterCharge = false;
			TeleporterIdleAnimation.StartOnProcess();
			if (unchargeOther)
			{
				LinkedTeleporter.Uncharge(false);
			}
		}

		public void SetCoordinate(int x, int y)
		{
			var tileSize = EmptyTile.GetSize();
			Position = new Vector2(LeftmostTileX + x * tileSize.X, TopmostTileY + y * tileSize.Y);
			TileCoordinate = new Vector2I(x, y);
		}

		public override void _Process(double delta)
		{
			if (MeltingAnimationNode != null)
			{
				ContinueMelt();
			}
			// water tile animation
			if (TileType == Type.Water)
			{
				WaterAnimation.Advance();
			}
			else if (TileType == Type.Teleporter && !IsPlaidTeleporter)
			{
				if (IsTeleporterBooting)
				{
					var ended = TeleporterBootingAnimation.Advance();
					if (ended)
					{
						IsTeleporterBooting = false;
						TeleporterIdleAnimation.StartOnProcess();
					}
				}
				else if (IsTeleporterCharge)
				{
					TeleporterChargeAnimation.Advance();
				}
				else
				{
					TeleporterIdleAnimation.Advance();
				}
			}
			else if (TileType == Type.Whirlpool)
			{
				WhirlpoolAnimation.Advance();
			}
		}

		/// <summary>
		/// Change the tile to a fresh new one of a given type
		/// </summary>
		public void ChangeTile(Type tileType)
		{
			RemoveKey();
			RemoveCoinBag();
			LinkedTeleporter = null;
			BlockReference = null;
			IsPlaidTeleporter = false;

			if (IsWhirlpool)
			{
				IsWhirlpool = false;
			}

			if (tileType == Type.Water)
			{
				WaterAnimation.StartOnProcess();
			}
			else if (tileType == Type.Teleporter)
			{
				TeleporterBootingAnimation.StartOnProcess();
				IsTeleporterBooting = true;
			}
			else if (tileType == Type.Whirlpool)
			{
				WhirlpoolAnimation.StartOnProcess();
				IsWhirlpool = true;
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

		/// <summary>
		/// When a puffle walking in the given direction (that is, moving from the opposite of the given direction)
		/// starts the animation to enter this tile
		/// </summary>
		public void OnPuffleStartEnter(Direction direction)
		{
			BlockReference?.Move(direction);

			// button is pressed on previous tile exit
			if (TileType == Type.Button)
			{
				Engine.PressSecretButton();
			}
		}

		/// <summary>
		/// Action to perform when a puffle fully finishes entering this tile
		/// </summary>
		public void OnPuffleFinishEnter(Puffle puffle)
		{
			if (CoinBag != null)
			{
				GetCoinBag();
			}
			var neighbors = Engine.GetNeighborTiles(this);
			// unlocking any adjacent locks if they exist
			if (puffle.HasKeys())
			{
				foreach ( var neighbor in neighbors )
				{
					if (neighbor.TileType == Type.Lock)
					{
						neighbor.ChangeTile(Type.Ice);
						puffle.UseKey();
						// ran out of keys
						if (!puffle.HasKeys())
						{
							break;
						}
					}
				}
			}
			foreach ( var neighbor in neighbors )
			{
				if (neighbor.TileType == Type.Teleporter)
				{
					neighbor.Charge();
				}
			}


			if (TileType == Type.Goal)
			{
				Engine.GoToNextLevel();
			}
			else if (!IsPlaidTeleporter && TileType == Type.Teleporter)
			{
				puffle.TeleportTo(LinkedTeleporter.TileCoordinate);
				MakePlaidTeleporter();
				LinkedTeleporter.MakePlaidTeleporter();
				if (puffle.IsStuck())
				{
					LinkedTeleporter.ChangeTile(Type.Whirlpool);
					puffle.Sink();
				}
			}
			else if (puffle.IsStuck())
			{
				ChangeTile(Type.Whirlpool);
				puffle.Sink();
			}

			if (KeyReference != null)
			{
				RemoveKey();
				puffle.GetKey();
			}
		}

		/// <summary>
		/// Action to perform when the puffle exits this tile (the moment the animation for leaving starts)
		/// </summary>
		public void OnPuffleExit()
		{
			// uncharges early, similar to original
			// might be interesting to uncharge after
			var neighbors = Engine.GetNeighborTiles(this);
			foreach ( var neighbor in neighbors )
			{
				if (neighbor.TileType == Type.Teleporter && neighbor.IsTeleporterCharge)
				{
					neighbor.Uncharge();
				}
			}
			if (TileType == Type.Ice || TileType == Type.ThickIce)
			{
				Type newType;
				if (TileType == Type.Ice)
				{
					newType = Type.Water;
					StartMelt();
				}
				else
				{
					newType = Type.Ice;
				}
				Engine.MeltTile();
				ChangeTile(newType);
			}
		}

		public void MakePlaidTeleporter()
		{
			IsPlaidTeleporter = true;
			Texture = PlaidTeleporterTile;
		}

		public void RemoveKey()
		{
			if (KeyReference != null)
			{
				KeyReference.QueueFree();
				KeyReference = null;
			}
		}

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

		public void GetCoinBag()
		{
			Engine.GetCoinBag();
			RemoveCoinBag();
		}
	}
}
