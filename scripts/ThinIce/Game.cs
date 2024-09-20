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
		public Texture2D KeyTexture { get; set; }

		[Export]
		public Texture2D BlockTexture { get; set; }

		[Export]
		public Texture2D CoinBagTexture { get; set; }

		public int CurrentLevelNumber { get; set; } = 1;

		/// <summary>
		/// How many points the player has at the start of each level
		/// </summary>
		public int PointsAtStartOfLevel { get; set; } = 0;

		/// <summary>
		/// How many points the player has collected in the current level in their
		/// current life
		/// </summary>
		public int PointsInLevel { get; set; } = 0;

		/// <summary>
		/// How many tiles the player has melted in the current level in their
		/// current life
		/// </summary>
		public int MeltedTiles { get; set; } = 0;

		/// <summary>
		/// How many times the player reset the current level
		/// </summary>
		public int TimesFailed { get; set; } = 0;

		public Level[] Levels { get; set; }

		public Level CurrentLevel => Levels[CurrentLevelNumber - 1];

		/// <summary>
		/// Grid containing all tiles used in the game
		/// </summary>
		public Tile[,] Tiles { get; set; }

		/// <summary>
		/// Reference to the Puffle object
		/// </summary>
		public Puffle Puffle { get; set; }

		/// <summary>
		/// A list containing references to all the blocks currently in the screen
		/// </summary>
		public List<Block> Blocks { get; set; }

		/// <summary>
		/// Whether or not previous level was solved
		/// </summary>
		public bool SolvedPrevious { get; set; } = false;

		public override void _Ready()
		{
			var parser = new LevelParser();
			Levels = parser.ParseLevels("res://assets/thin_ice/levels.json");

			Tiles = new Tile[Level.MaxWidth, Level.MaxHeight];
			Blocks = new List<Block>();

			// reference hardcoded values
			int leftmostTileX = -2158;
			int topmostTileY = -1700;



			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					var tile = GD.Load<PackedScene>("res://scenes/thin_ice/tile.tscn").Instantiate<Tile>();
					tile.Texture = tile.EmptyTile;
					var tileSize = tile.Texture.GetSize();
					tile.Position = new Vector2(leftmostTileX + i * tileSize.X, topmostTileY + j * tileSize.Y);
					tile.TileCoordinate = new Vector2I(i, j);
					tile.Game = this;
					Tiles[i, j] = tile;
					AddChild(tile);
					// move up to so the pre-created nodes are visible
					MoveChild(tile, 0);
				}
			}

			// move to bottom so it is always visible on top of tiles
			Puffle = GetNode<Puffle>("Puffle");
		}

		/// <summary>
		/// All valid tile types
		/// </summary>
		public enum TileType
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

		/// <summary>
		/// Class for a Thin Ice level's layout
		/// </summary>
		public class Level
		{
			public static readonly int MaxWidth = 19;

			public static readonly int MaxHeight = 15;

			/// <summary>
			/// Tiles that do not count towards the total tile count
			/// </summary>
			public static readonly List<TileType> ZeroCountTiles = new()
			{
				TileType.Empty,
				TileType.Water,
				TileType.Wall,
				TileType.BlockHole,
				TileType.Goal,
				TileType.Teleporter,
				TileType.FakeTemporaryWall,
				TileType.Button
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
			public TileType[,] Tiles { get; set; }

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

			public Level()
			{
			}

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
							TileType tileType = Tiles[i, j];
							if (!ZeroCountTiles.Contains(tileType))
							{
								count++;
								if (tileType == TileType.ThickIce)
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

		/// <summary>
		/// Draws the current level to its absolute freshest state
		/// </summary>
		public void DrawLevel()
		{
			ClearBlocks();
			Level level = CurrentLevel;
			int teleporterCount = 0;
			Tile lastTeleporter = null;
			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					TileType tileType;
					if (level.IsPointOutOfBounds(new Vector2I(i, j)))
					{
						tileType = TileType.Empty;
					}
					else
					{
						tileType = level.Tiles[i - level.RelativeOrigin.X, j - level.RelativeOrigin.Y];
					}
					Tiles[i, j].ChangeTile(tileType);
					Tile currentTile = Tiles[i, j];

					// teleporters are currently linked in pair based
					// on first appearance
					// works for vanilla ones, but might be expanded for custom ones in the future
					if (tileType == TileType.Teleporter)
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
			foreach (Vector2I keyPosition in level.KeyPositions)
			{
				Tiles[keyPosition.X, keyPosition.Y].AddKey();
			}
			foreach (Vector2I blockPosition in level.BlockPositions)
			{
				Tile blockTile = Tiles[blockPosition.X, blockPosition.Y];
				Block block = new()
				{
					Texture = BlockTexture,
					Coordinates = blockPosition,
					Position = blockTile.Position
				};
				AddChild(block);
				Blocks.Add(block);
				blockTile.BlockReference = block;
			}
			if (SolvedPrevious && level.CoinBagPosition != null)
			{
				Tile tile = GetTile((Vector2I)level.CoinBagPosition);
				Sprite2D coinBag = new()
				{
					Texture = CoinBagTexture
				};

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
			Puffle.TeleportTo(CurrentLevel.PuffleSpawnLocation);
			Puffle.ResetKeys();
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
		public void ClearBlocks()
		{
			foreach (Block block in Blocks)
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
					if (tile.TileType == TileType.FakeTemporaryWall)
					{
						tile.ChangeTile(TileType.Wall);
					}
					else if (tile.TileType == TileType.FakePassableWall || tile.TileType == TileType.FakeImpassableWall)
					{
						tile.ChangeTile(TileType.Ice);
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
		/// Signal for when the reset button is pressed
		/// </summary>
		private void OnResetButtonPressed()
		{
			ResetLevel();
			Puffle.Die();
		}

		/// <summary>
		/// Action for when a tile is melted
		/// </summary>
		public void MeltTile()
		{
			MeltedTiles++;
			PointsInLevel++;
		}

		/// <summary>
		/// Gets the total number of points the player has to display
		/// </summary>
		/// <returns></returns>
		public int GetPoints()
		{
			return PointsAtStartOfLevel + PointsInLevel;
		}
	}
}
