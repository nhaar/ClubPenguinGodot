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
		public Texture2D KeyTexture { get; set; }

		[Export]
		public Texture2D BlockTexture { get; set; }

		[Export]
		public Texture2D CoinBagTexture { get; set; }

		[Export]
		public SpriteFrames WaterTileFrames { get; set; }

		[Export]
		public SpriteFrames TeleporterTileFrames { get; set; }

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
			Levels = parser.ParseLevels("res://assets/thin_ice/levels.xml");

			Tiles = new Tile[Level.MaxWidth, Level.MaxHeight];
			Blocks = new List<Block>();
			Vector2 tileSize = EmptyTile.GetSize();

			// reference hardcoded values
			int leftmostTileX = -2158;
			int topmostTileY = -1700;

			for (int i = 0; i < Level.MaxWidth; i++)
			{
				for (int j = 0; j < Level.MaxHeight; j++)
				{
					Tile newTile = new()
					{
						Texture = EmptyTile,
						Position = new Vector2(leftmostTileX + i * tileSize.X, topmostTileY + j * tileSize.Y),
						TileCoordinate = new Vector2I(i, j),
						Game = this
					};
					Tiles[i, j] = newTile;
					AddChild(newTile);
					// move up to so the pre-created nodes are visible
					MoveChild(newTile, 0);
				}
			}

			// move to bottom so it is always visible on top of tiles
			Puffle = (Puffle)GetNode("ThinIcePuffle");
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

			// level string format
			// map
			// comma separated rows define tileset
			// ;
			// keys(coord),(coord)
			// blocks(coord),(coord)
			// puffle(coord)
			// origin(coord)
			// dim(width,height)
			/// <summary>
			/// Initialize a level from the custom level string format
			/// </summary>
			/// <param name="levelString">Custom level string that follows this format:
			/// map
			/// (mapString, see <c>MapDecode</c>);
			/// origin(x,y)
			/// puffle(x,y)
			/// keys(x,y),...
			/// blocks(x,y),...
			/// 
			/// Where the map string defines the tileset,
			/// the origin defines the relative origin,
			/// the puffle defines the spawn location,
			/// and keys and blocks define coordinates of where the keys and blocks are placed.
			/// </param>
			/// <param name="altPatches">
			/// If this level has an alternative version, this is the list of patches that should be applied.
			/// See more about the format in <c>Patch</c>
			/// </param>
			public Level(string levelString, params string[] altPatches)
			{
				//Tiles = MapDecodeInsideString(levelString);
				//KeyPositions = GetCoordsFromString(GetInlineCoordsFromWord(levelString, "keys"));
				//BlockPositions = GetCoordsFromString(GetInlineCoordsFromWord(levelString, "blocks"));
				//PuffleSpawnLocation = (Vector2I)GetCoordFromString(GetInlineCoordsFromWord(levelString, "puffle"));
				//RelativeOrigin = GetCoordFromString(GetInlineCoordsFromWord(levelString, "origin")) ?? Vector2I.Zero;
				//CoinBagPosition = GetCoordFromString(GetInlineCoordsFromWord(levelString, "bag"));
				//Width = Tiles.GetLength(0);
				//Height = Tiles.GetLength(1);

				//// 50/50 as per original game
				//if (altPatches.Length > 0 && GD.Randf() > 0.5)
				//{
				//	foreach (string patchString in altPatches)
				//	{
				//		ApplyPatch(new Patch(patchString));
				//	}
				//}
			}

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
			/// Helper function that given a string that contains a list of coordinate pairs like
			/// (x,y),(x,y),...
			/// returns the list of coordinate pairs
			/// </summary>
			/// <param name="coords"></param>
			/// <returns></returns>
			public static List<Vector2I> GetCoordsFromString(string coords)
			{
				List<Vector2I> coordsList = new();
				MatchCollection matches = Regex.Matches(coords, @"\(\d+,\d+\)");
				foreach (Match match in matches)
				{
					Match coordMatch = Regex.Match(match.Value, @"(?<x>\d+),(?<y>\d+)");
					coordsList.Add(new Vector2I(int.Parse(coordMatch.Groups["x"].Value), int.Parse(coordMatch.Groups["y"].Value)));
				}

				return coordsList;
			}

			/// <summary>
			/// Helper function that given a string that contains a single coordinate pair like
			/// (x,y)
			/// returns that coordinate pair, or null if it doesn't have pairs
			/// </summary>
			/// <param name="coord"></param>
			/// <returns></returns>
			public static Vector2I? GetCoordFromString(string coord)
			{
				List<Vector2I> coordsList = GetCoordsFromString(coord);
				if (coordsList.Count < 1)
				{
					return null;
				}
				return coordsList[0];
			}

			/// <summary>
			/// Decode the custom tileset notation into a tile grid
			/// </summary>
			/// <param name="mapString">
			/// A comma separated representation of the <c>Tiles</c> grid,
			/// where each row is separated by a newline, and each tile
			/// is separated by a comma. The tile format is
			/// using the format <c>(amount)(tile)</c>, where <c>amount</c>
			/// can be ommited and will be count as 1, or otherwise represents the number
			/// of tiles in a row, and the tile name is a string representing
			/// the tile type (see function body)
			/// </param>
			/// <returns></returns>
			/// <exception cref="Exception"></exception>
			public static TileType[,] MapDecode(string mapString)
			{
				List<string[]> records = ParseCsv(mapString.Trim());

				List<List<TileType>> tileMap = new();
				foreach (string[] record in records)
				{
					List<TileType> row = new();
					foreach (string tile in record)
					{
						Match tileMatch = Regex.Match(tile, @"(?<amount>[\d]*)(?<tile>[a-zA-Z]+)");
						if (tileMatch.Success)
						{
							TileType tileType = tileMatch.Groups["tile"].Value switch
							{
								"empty" => TileType.Empty,
								"ice" => TileType.Ice,
								"water" => TileType.Water,
								"thick" => TileType.ThickIce,
								"wall" => TileType.Wall,
								"goal" => TileType.Goal,
								"tp" => TileType.Teleporter,
								"plaid" => TileType.PlaidTeleporter,
								"lock" => TileType.Lock,
								"button" => TileType.Button,
								"faketemp" => TileType.FakeTemporaryWall,
								"fakepass" => TileType.FakePassableWall,
								"fakeimpass" => TileType.FakeImpassableWall,
								"hole" => TileType.BlockHole,
								_ => throw new Exception("Invalid tile type: " + tile)
							};
							string amountString = tileMatch.Groups["amount"].Value;
							int amount = amountString == string.Empty ? 1 : int.Parse(tileMatch.Groups["amount"].Value);
							for (int i = 0; i < amount; i++)
							{
								row.Add(tileType);
							}
						}
						else
						{
							throw new Exception($"Invalid tile type: {tile} in level string\n{mapString}");
						}
					}
					tileMap.Add(row);
				}

				// we transpose so that horizontal is first argument and vertical is second
				TileType[,] tiles = new TileType[tileMap[0].Count, tileMap.Count];
				for (int i = 0; i < tileMap.Count; i++)
				{
					for (int j = 0; j < tileMap[i].Count; j++)
					{
						try
						{
							tiles[j, i] = tileMap[i][j];
						}
						catch
						{
							throw new Exception($"Incorrect tile map (error in {j},{i}) :\n" + mapString);
						}
					}
				}

				return tiles;
			}

			/// <summary>
			/// Applies <c>MapDecode</c> to a string that contains a map definition somewhere.
			/// </summary>
			/// <param name="levelString"></param>
			/// <returns></returns>
			public static TileType[,] MapDecodeInsideString(string levelString)
			{
				return MapDecode(Regex.Match(levelString, @"(?<=map(\r\n|\n))[^;]+").Value);
			}

			/// <summary>
			/// Get the line that consists of a word and everything after it from inside a string.
			/// Used as a helper function to get the coordinates from the level string for each keyword.
			/// </summary>
			/// <param name="levelString">String to search in.</param>
			/// <param name="word">Word to find at the start of line.</param>
			/// <returns></returns>
			public static string GetInlineCoordsFromWord(string levelString, string word)
			{
				return Regex.Match(levelString, $@"(?<={word}).*").Value;
			}

			/// <summary>
			/// Helper function that parses a csv-like string into a list of string arrays
			/// that represent each row
			/// </summary>
			/// <param name="csv"></param>
			/// <returns></returns>
			public static List<string[]> ParseCsv(string csv)
			{
				List<string[]> records = new List<string[]>();

				string[] lines = csv.Split('\n');

				for (int i = 0; i < lines.Length; i++)
				{
					string[] fields = lines[i].Split(',');
					fields = fields.Select(field => field.Trim()).ToArray();
					records.Add(fields);
				}

				return records;
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

			/// <summary>
			/// Class for a patch that can be applied to a level
			/// and modify it.
			/// As the changes in the original game, the only possible changes are tile changes,
			/// any other properties (keys, bags, etc) can't be modified.
			/// </summary
			public class Patch
			{
				/// <summary>
				/// Where the patch should start being applied, using absolute coordinates of the grid
				/// </summary>
				public Vector2I Origin { get; private set; }

				/// <summary>
				/// A grid containing all the tiles that the patch will change
				/// </summary>
				public TileType[,] Tiles { get; private set; }

				/// <summary>
				/// Size of the patch tile grid
				/// </summary>
				public Vector2I Size
				{
					get
					{
						return new(Tiles.GetLength(0), Tiles.GetLength(1));
					}
				}

				/// <summary>
				/// Create a patch from the string.
				/// </summary>
				/// <param name="patchString">
				/// The string follow the same format as the normal level string, but it only includes
				/// the origin of the patch and the tile map.
				/// </param>
				public Patch(string patchString)
				{
					Tiles = MapDecodeInsideString(patchString);
					Origin = GetCoordFromString(GetInlineCoordsFromWord(patchString, "origin")) ?? Vector2I.Zero;
				}
			}

			/// <summary>
			/// Modifies this level according to a patch.
			/// </summary>
			/// <param name="patch"></param>
			public void ApplyPatch(Patch patch)
			{
				// expanding the tile size in case the patch requires it
				Vector2I requiredSize = patch.Size - RelativeOrigin + patch.Origin;
				if (requiredSize.X > Width || requiredSize.Y > Height)
				{
					Vector2I newSize = new(Math.Max(requiredSize.X, Width), Math.Max(requiredSize.Y, Height));
					TileType[,] newTiles = new TileType[newSize.X, newSize.Y];
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{
							newTiles[i, j] = Tiles[i, j];
						}
					}
					Width = newSize.X;
					Height = newSize.Y;
					Tiles = newTiles;
				}

				for (int i = 0; i < patch.Size.X; i++)
				{
					for (int j = 0; j < patch.Size.Y; j++)
					{
						Tiles[i + patch.Origin.X - RelativeOrigin.X, j + patch.Origin.Y - RelativeOrigin.Y] = patch.Tiles[i, j];
					}
				}
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
