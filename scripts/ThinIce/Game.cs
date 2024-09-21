using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Node for the contents of the Thin Ice game scene, which consitutes of the
	/// everything in the arcade screen after the "PLAY" button is pressed.
	/// </summary>
	public partial class Game : Node2D
	{
		[Export]
		public PackedScene TileScene { get; set; }

		[Export]
		public PackedScene BlockScene { get; set; }

		[Export]
		public PackedScene CoinBagScene { get; set; }

		[Export]
		public NodePath PufflePath { get; set; }

		public int CurrentLevelNumber { get; private set; } = 1;

		public int TotalTileCount => CurrentLevel.TotalTileCount;

		/// <summary>
		/// How many points the player has at the start of each level
		/// </summary>
		private int PointsAtStartOfLevel { get; set; } = 0;

		/// <summary>
		/// How many points the player has collected in the current level in their
		/// current life
		/// </summary>
		public int PointsInLevel { get; set; } = 0;

		/// <summary>
		/// How many tiles the player has melted in the current level in their
		/// current life
		/// </summary>
		public int MeltedTiles { get; private set; } = 0;

		/// <summary>
		/// Gets the total number of points the player has to display
		/// </summary>
		/// <returns></returns>
		public int Points => PointsAtStartOfLevel + PointsInLevel;

		/// <summary>
		/// How many times the player reset the current level
		/// </summary>
		private int TimesFailed { get; set; } = 0;

		private Level[] Levels { get; set; }

		private Level CurrentLevel => Levels[CurrentLevelNumber - 1];

		/// <summary>
		/// Grid containing all tiles used in the game
		/// </summary>
		private Tile[,] Tiles { get; set; }

		/// <summary>
		/// Reference to the Puffle object
		/// </summary>
		private Puffle Puffle { get; set; }

		/// <summary>
		/// A list containing references to all the blocks currently in the screen
		/// </summary>
		private List<Block> Blocks { get; set; }

		/// <summary>
		/// Whether or not previous level was solved
		/// </summary>
		private bool SolvedPrevious { get; set; } = false;


		public override void _Ready()
		{ 
			var parser = new LevelParser();
			Levels = parser.ParseLevels("res://assets/thin_ice/levels.json");

			Tiles = new Tile[Level.MaxWidth, Level.MaxHeight];
			Blocks = new List<Block>();

			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					var tile = TileScene.Instantiate<Tile>();
					tile.Game = this;
					tile.SetCoordinate(i, j);
					Tiles[i, j] = tile;
					AddChild(tile);
				}
			}

			// move to bottom so it is always visible on top of tiles
			Puffle = GetNode<Puffle>(PufflePath);
			Puffle.Game = this;
		}

		public void MakeVisible()
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
				Tiles[keyPosition.X, keyPosition.Y].AddKey();
			}
			foreach (var blockPosition in level.BlockPositions)
			{
				var blockTile = Tiles[blockPosition.X, blockPosition.Y];
				var block = BlockScene.Instantiate<Block>();
				block.Coordinates = blockPosition;
				block.Position = blockTile.Position;
				block.Game = this;
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
		/// <param name="levelNumber"></param>
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

		/// <summary>
		/// Clears all blocks from the screen
		/// </summary>
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
		/// <param name="coordinates"></param>
		/// <returns></returns>
		public Tile GetTile(Vector2I coordinates)
		{
			return Tiles[coordinates.X, coordinates.Y];
		}

		/// <summary>
		/// Action for when the button that exposes fake walls is pressed
		/// </summary>
		public void PressButton()
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
		/// Resets the level
		/// </summary>
		public void ResetLevel()
		{
			TimesFailed++;
			StartLevel(CurrentLevelNumber, true);
		}

		/// <summary>
		/// Action for when a tile is melted
		/// </summary>
		public void MeltTile()
		{
			MeltedTiles++;
			PointsInLevel++;
		}
	}
}
