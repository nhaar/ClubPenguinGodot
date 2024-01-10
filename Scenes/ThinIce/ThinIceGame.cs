using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClubPenguinPlus.ThinIce;

/// <summary>
/// Node for the contents of the Thin Ice game scene, which consitutes of the
/// everything in the arcade screen after the "PLAY" button is pressed.
/// </summary>
public partial class ThinIceGame : Node2D
{
	[Export]
	public Texture2D EmptyTile { get; set; }

	[Export]
	public Texture2D IceTile { get; set; }

	[Export]
	public Texture2D WaterTile { get; set; }

	[Export]
	public Texture2D ThickIceTile { get; set; }

	[Export]
	public Texture2D WallTile { get; set; }

	[Export]
	public Texture2D GoalTile { get; set; }

	[Export]
	public Texture2D TeleporterTile { get; set; }

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

	public int CurrentLevelNumber { get; set; } = 1;

	public Level CurrentLevel => ThinIceLevels.Levels[CurrentLevelNumber - 1];

	/// <summary>
	/// Grid containing all tiles used in the game
	/// </summary>
	public ThinIceTile[,] Tiles { get; set; }

	/// <summary>
	/// Reference to the Puffle object
	/// </summary>
	public ThinIcePuffle Puffle { get; set; }

	/// <summary>
	/// A list containing references to all the blocks currently in the screen
	/// </summary>
	public List<ThinIceBlock> Blocks { get; set; }

	public override void _Ready()
	{
		Tiles = new ThinIceTile[Level.MaxWidth, Level.MaxHeight];
		Blocks = new List<ThinIceBlock>();
		Vector2 tileSize = EmptyTile.GetSize();

		// reference hardcoded values
		int leftmostTileX = -2100;
		int topmostTileY = -1500;

		for (int i = 0; i < Level.MaxWidth; i++)
		{
			for (int j = 0; j < Level.MaxHeight; j++)
			{
				ThinIceTile newTile = new()
				{
					Texture = EmptyTile,
					Position = new Vector2(leftmostTileX + i * tileSize.X, topmostTileY + j * tileSize.Y),
					TileCoordinate = new Vector2I(i, j),
					Game = this
				};
				Tiles[i, j] = newTile;
				AddChild(newTile);
			}
		}

		// move to bottom so it is always visible on top of tiles
		Puffle = (ThinIcePuffle)GetNode("ThinIcePuffle");
		MoveChild(Puffle, -1);
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
			TileType.BlockHole
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
		public Level(string levelString)
		{
			Tiles = MapDecode(Regex.Match(levelString, @"(?<=map(\r\n|\n))[^;]+").Value);
			KeyPositions = GetCoordsFromString(Regex.Match(levelString, @"(?<=keys).*").Value);
			BlockPositions = GetCoordsFromString(Regex.Match(levelString, @"(?<=blocks).*").Value);
			PuffleSpawnLocation = (Vector2I)GetCoordFromString(Regex.Match(levelString, @"(?<=puffle).*").Value);
			RelativeOrigin = GetCoordFromString(Regex.Match(levelString, @"(?<=origin).*").Value) ?? Vector2I.Zero;
			Width = Tiles.GetLength(0);
			Height = Tiles.GetLength(1);
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
	}

	/// <summary>
	/// Draws the current level to its absolute freshest state
	/// </summary>
	public void DrawLevel()
	{
		ClearBlocks();
		Level level = CurrentLevel;
		int teleporterCount = 0;
		ThinIceTile lastTeleporter = null;
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
				ThinIceTile currentTile = Tiles[i, j];

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
			ThinIceTile blockTile = Tiles[blockPosition.X, blockPosition.Y];
			ThinIceBlock block = new()
			{
				Texture = BlockTexture,
				Coordinates = blockPosition,
				Position = blockTile.Position
			};
			AddChild(block);
			Blocks.Add(block);
			blockTile.BlockReference = block;
		}
	}

	/// <summary>
	/// Start level based on its number
	/// </summary>
	/// <param name="levelNumber"></param>
	public void StartLevel(int levelNumber)
	{
		CurrentLevelNumber = levelNumber;
		DrawLevel();
		Puffle.TeleportTo(CurrentLevel.PuffleSpawnLocation);
		Puffle.ResetKeys();
	}

	public void GoToNextLevel()
	{
		StartLevel(CurrentLevelNumber + 1);
	}

	/// <summary>
	/// Clears all blocks from the screen
	/// </summary>
	public void ClearBlocks()
	{
		foreach(ThinIceBlock block in Blocks)
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
	public ThinIceTile GetTile(Vector2I coordinates)
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
				ThinIceTile tile = Tiles[i, j];
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
}
