using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ClubPenguinPlus.ThinIce;

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

	public ThinIceTile[,] Tiles { get; set; }

	public ThinIcePuffle Puffle { get; set; }

	public List<ThinIceBlock> Blocks { get; set; }

	public override void _Ready()
	{
		Tiles = new ThinIceTile[Level.MaxWidth, Level.MaxHeight];
		Blocks = new List<ThinIceBlock>();
		Vector2 tileSize = EmptyTile.GetSize();

		for (int i = 0; i < Level.MaxWidth; i++)
		{
			for (int j = 0; j < Level.MaxHeight; j++)
			{
				ThinIceTile newTile = new();
				newTile.Texture = EmptyTile;
				newTile.Position = new Vector2(-2100 + i * tileSize.X, - 1500 + j * tileSize.Y);
				newTile.TileCoordinate = new Vector2I(i, j);
				newTile.Game = this;
				Tiles[i, j] = newTile;
				AddChild(newTile);
			}
		}

		Puffle = (ThinIcePuffle)GetNode("ThinIcePuffle");
		// move to bottom so it is always on top of tiles
		MoveChild(Puffle, -1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public enum TileType
	{
		Empty,
		Ice,
		Water,
		ThickIce,
		Wall,
		Goal,
		Teleporter,
		PlaidTeleporter,
		Lock,
		Button,
		FakeWall,
		BlockHole
	}

	public class Level
	{
		public static readonly int MaxWidth = 19;

		public static readonly int MaxHeight = 15;

		public static readonly List<TileType> ZeroCountTiles = new()
		{
			TileType.Empty,
			TileType.Water,
			TileType.Wall,
			TileType.BlockHole
		};

		public Vector2I RelativeOrigin { get; set; }

		// all others coordinates are absolute coords
		public Vector2I PuffleSpawnLocation { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public TileType[,] Tiles { get; set; }

		public List<Vector2I> KeyPositions { get; set; }

		public List<Vector2I> BlockPositions { get; set; }
	
		public Level(TileType[,] tiles, Vector2I puffleSpawnLocation, Vector2I origin, List<Vector2I> keyPositions = null, List<Vector2I> blockPositions = null)
		{
			Tiles = tiles;
			PuffleSpawnLocation = puffleSpawnLocation;
			RelativeOrigin = origin;
			Width = tiles.GetLength(0);
			Height = tiles.GetLength(1);
			KeyPositions = keyPositions ?? new List<Vector2I>();
			BlockPositions = blockPositions ?? new List<Vector2I>();
		}

		// level string format
		// map
		// comma separated rows define tileset
		// ;
		// keys(coord),(coord)
		// blocks(coord),(coord)
		// puffle(coord)
		// origin(coord)
		// dim(width,height)
		public Level(string levelString)
		{
			Match match = Regex.Match(levelString, @"(?<map>(?<=map\n)[^;]+)|(?<keys>(?<=keys).*);(?<blocks>(?<=blocks).*)(?<puffle>(?<=puffle).*)(?<origin>(?<=origin).*)(?<dim>(?<=dim).*)");

			Tiles = MapDecode(Regex.Match(levelString, @"(?<=map(\r\n|\n))[^;]+").Value);
			KeyPositions = GetCoordsFromString(Regex.Match(levelString, @"(?<=keys).*").Value);
			BlockPositions = GetCoordsFromString(Regex.Match(levelString, @"(?<=blocks).*").Value);
			PuffleSpawnLocation = (Vector2I)GetCoordFromString(Regex.Match(levelString, @"(?<=puffle).*").Value);
			RelativeOrigin = GetCoordFromString(Regex.Match(levelString, @"(?<=origin).*").Value) ?? Vector2I.Zero;
			Width = Tiles.GetLength(0);
			Height = Tiles.GetLength(1);
		}

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

		public static Vector2I? GetCoordFromString(string coord)
		{
			List<Vector2I> coordsList = GetCoordsFromString(coord);
			if (coordsList.Count < 1)
			{
				return null;
			}
			return coordsList[0];
		}

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
							"fake" => TileType.FakeWall,
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

		public bool IsPointOutOfBounds(Vector2I point)
		{
			return point.X < RelativeOrigin.X || point.Y < RelativeOrigin.Y || point.X >= Width + RelativeOrigin.X || point.Y >= Height + RelativeOrigin.Y;
		}
	}

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
			ThinIceBlock block = new();
			block.Texture = BlockTexture;
			block.Coordinates = blockPosition;
			ThinIceTile blockTile = Tiles[blockPosition.X, blockPosition.Y];
			block.Position = blockTile.Position;
			AddChild(block);
			Blocks.Add(block);
			blockTile.BlockReference = block;
		}
	}

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

	public void ClearBlocks()
	{
		foreach(ThinIceBlock block in Blocks)
		{
			block.QueueFree();
		}
		Blocks.Clear();
	}
}
