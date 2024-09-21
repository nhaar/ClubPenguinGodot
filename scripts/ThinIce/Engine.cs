using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Node for the game engine
	/// </summary>
	public partial class Engine : Node2D
	{
		[Export]
		private PackedScene TileScene { get; set; }

		[Export]
		private PackedScene BlockScene { get; set; }

		[Export]
		private PackedScene CoinBagScene { get; set; }

		[Export]
		private NodePath PufflePath { get; set; }

		[Export]
		private Json LevelsJson { get; set; }

		public int CurrentLevelNumber { get; private set; } = 1;

		/// <summary>
		/// Total tiles available to melt in current level
		/// </summary>
		public int TotalTileCount => CurrentLevel.TotalTileCount;

		/// <summary>
		/// How many points the player has at the start of each level
		/// </summary>
		private int PointsAtStartOfLevel { get; set; } = 0;

		/// <summary>
		/// How many points the player has collected in the current level in their
		/// current life
		/// </summary>
		private int PointsInLevel { get; set; } = 0;

		/// <summary>
		/// How many tiles the player has melted in the current level in their
		/// current life
		/// </summary>
		public int MeltedTiles { get; private set; } = 0;

		/// <summary>
		/// Gets the total number of points the player has to display
		/// </summary>
		public int Points => PointsAtStartOfLevel + PointsInLevel;

		/// <summary>
		/// How many times the player reset the current level
		/// </summary>
		private int TimesFailed { get; set; } = 0;

		/// <summary>
		/// All game levels in order
		/// </summary>
		private Level[] Levels { get; set; }

		private Level CurrentLevel => Levels[CurrentLevelNumber - 1];

		/// <summary>
		/// The game tile map
		/// </summary>
		private Tile[,] Tiles { get; set; }

		private Puffle Puffle { get; set; }

		/// <summary>
		/// All blocks in the game
		/// </summary>
		private List<Block> Blocks { get; set; }

		/// <summary>
		/// Whether or not previous level was solved
		/// </summary>
		private bool SolvedPrevious { get; set; } = false;

		public override void _Ready()
		{
			var parser = new LevelParser();
			Levels = parser.ParseLevels(LevelsJson);

			Tiles = new Tile[Level.MaxWidth, Level.MaxHeight];
			Blocks = new List<Block>();

			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					var tile = TileScene.Instantiate<Tile>();
					tile.Engine = this;
					tile.SetCoordinate(i, j);
					Tiles[i, j] = tile;
					AddChild(tile);
				}
			}

			// move to bottom so it is always visible on top of tiles
			Puffle = GetNode<Puffle>(PufflePath);
			Puffle.Engine = this;
		}

		/// <summary>
		/// Makes this engine instance ready to play
		/// </summary>
		public void Activate()
		{
			Visible = true;
			Puffle.Activate();
			StartLevel(1);
		}

		/// <summary>
		/// Draws the current level to its absolute freshest state
		/// </summary>
		private void DrawLevel()
		{
			ClearBlocks();
			var level = CurrentLevel;
			int teleporterCount = 0;
			Tile lastTeleporter = null;
			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					Tile.Type tileType;
					if (level.IsPointOutOfBounds(new Vector2I(i, j)))
					{
						tileType = Tile.Type.Empty;
					}
					else
					{
						tileType = level.Tiles[i - level.RelativeOrigin.X, j - level.RelativeOrigin.Y];
					}
					Tiles[i, j].ChangeTile(tileType);
					var currentTile = Tiles[i, j];

					// teleporters are currently linked in pair based
					// on first appearance
					// works for vanilla ones, but might be expanded for custom ones in the future
					if (tileType == Tile.Type.Teleporter)
					{
						if (teleporterCount % 2 == 0)
						{
							lastTeleporter = currentTile;
						}
						else
						{
							currentTile.LinkedTeleporter = lastTeleporter;
							lastTeleporter.LinkedTeleporter = currentTile;
							lastTeleporter = null;
						}
						teleporterCount++;
					}
				}
			}
			foreach (var keyPosition in level.KeyPositions)
			{
				GetTile(keyPosition).AddKey();
			}
			foreach (var blockPosition in level.BlockPositions)
			{
				var blockTile = GetTile(blockPosition);
				var block = BlockScene.Instantiate<Block>();
				block.Coordinates = blockPosition;
				block.Position = blockTile.Position;
				block.Engine = this;
				AddChild(block);
				Blocks.Add(block);
				blockTile.BlockReference = block;
			}
			if (SolvedPrevious && level.CoinBagPosition != null)
			{
				var tile = GetTile((Vector2I)level.CoinBagPosition);
				var coinBag = CoinBagScene.Instantiate<Sprite2D>();
				tile.AddChild(coinBag);
				tile.CoinBag = coinBag;
			}
		}

		/// <summary>
		/// Start level based on its number
		/// </summary>
		/// <param name="resetting">
		/// Whether or not the player is resetting the level
		/// </param>
		public void StartLevel(int levelNumber, bool resetting = false)
		{
			if (!resetting)
			{
				TimesFailed = 0;
			}
			PointsInLevel = 0;
			MeltedTiles = 0;

			CurrentLevelNumber = levelNumber;
			DrawLevel();
			Puffle.Die();
			Puffle.TeleportTo(CurrentLevel.PuffleSpawnLocation);
		}

		public void GoToNextLevel()
		{
			bool solved = MeltedTiles == CurrentLevel.TotalTileCount;
			if (solved)
			{
				// double if first try
				int factor = TimesFailed == 0 ? 2 : 1;
				PointsInLevel += factor * CurrentLevel.TotalTileCount;
			}
			SolvedPrevious = solved;
			PointsAtStartOfLevel += PointsInLevel;
			StartLevel(CurrentLevelNumber + 1);
		}

		private void ClearBlocks()
		{
			foreach (var block in Blocks)
			{
				block.QueueFree();
				GetTile(block.Coordinates).BlockReference = null;
			}
			Blocks.Clear();
		}

		/// <summary>
		/// Gets tile in the given coordinate
		/// </summary>
		public Tile GetTile(Vector2I coordinates)
		{
			return Tiles[coordinates.X, coordinates.Y];
		}

		/// <summary>
		/// Action for when the button that exposes fake walls is pressed (level 19 hidden area)
		/// </summary>
		public void PressSecretButton()
		{
			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					Tile tile = Tiles[i, j];
					if (tile.TileType == Tile.Type.FakeTemporaryWall)
					{
						tile.ChangeTile(Tile.Type.Wall);
					}
					else if (tile.TileType == Tile.Type.FakePassableWall || tile.TileType == Tile.Type.FakeImpassableWall)
					{
						tile.ChangeTile(Tile.Type.Ice);
					}
				}
			}
		}

		/// <summary>
		/// Restarting the level
		/// </summary>
		public void ResetLevel()
		{
			TimesFailed++;
			StartLevel(CurrentLevelNumber, true);
		}

		public void MeltTile()
		{
			MeltedTiles++;
			PointsInLevel++;
		}

		public void GetCoinBag()
		{
			PointsInLevel += 100;
		}
	}
}
