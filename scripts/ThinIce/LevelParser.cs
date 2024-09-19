using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClubPenguinPlus.ThinIce
{
	public partial class LevelParser : XmlParser
	{
		public Game.Level[] ParseLevels(string path)
		{
			List<Game.Level> levels = new();
			Open(path);
			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					if (nodeName == "level")
					{
						levels.Add(ParseLevel());
					}
					else if (nodeName != "levels")
					{
						throw new Exception("Unknown node name in levels file: " + nodeName);
					}
				}
			}

			return levels.ToArray();
		}
	
		public Game.Level ParseLevel()
		{
			// 50/50 as per the original game
			var useAlt = GD.Randf() > -1;

			var size = new Vector2I(Game.Level.MaxWidth, Game.Level.MaxHeight);
			var origin = Vector2I.Zero;
			var spawn = -1 * Vector2I.One;
			var gotRows = false;
			var tileMap = new List<List<Game.TileType>>();
			var gotMap = false;
			Vector2I? coinBag = null;
			var keys = new List<Vector2I>();
			var blocks = new List<Vector2I>();

			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					var attributesDict = new Dictionary<string, string>();
					for (int idx = 0; idx < GetAttributeCount(); idx++)
					{
						attributesDict[GetAttributeName(idx)] = GetAttributeValue(idx);
					}
					switch (nodeName)
					{
						case "size":
							if (gotRows)
							{
								throw new Exception("Size was set AFTER rows in level. Put them before");
							}
							size = ParseSizeElement(attributesDict, size);
							break;
						case "origin":
							origin = ParseGridCoordinateElement(attributesDict, origin);
							break;
						case "spawn":
							spawn = ParseGridCoordinateElement(attributesDict, spawn);
							if (spawn.X == -1 || spawn.Y == -1)
							{
								throw new Exception("Puffle spawn coordinates not given");
							}
							break;
						case "rows":
							tileMap = ParseRows(size.X, size.Y);
							gotMap = true;
							break;
						case "bag":
							coinBag = ParseGridCoordinateElement(attributesDict, Vector2I.Zero);
							break;
						case "keys":
							keys = ParseCoordinateList("keys", "key");
							break;
						case "blocks":
							blocks = ParseCoordinateList("blocks", "block");
							break;
						case "alt":
							if (!gotMap)
							{
								throw new Exception("Alt segment placed before levels");
							}
							if (useAlt)
							{
								tileMap = ParseAndApplyAlt(tileMap, origin);
							}
							else
							{
								SkipAlt();
							}
							break;
						default:
							throw new Exception($"Unexpected level element: {nodeName}, either remove it or place it afte rows");
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "level")
				{
					break;
				}
			}

			
			var tiles = new Game.TileType[tileMap[0].Count, tileMap.Count];
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
						throw new Exception($"Incorrect tile map (error in {j},{i}) :\n");
					}
				}
			}

			return new Game.Level
			{
				RelativeOrigin = origin,
				PuffleSpawnLocation = spawn,
				Width = size.X,
				Height = size.Y,
				Tiles = tiles,
				KeyPositions = keys,
				BlockPositions = blocks,
				CoinBagPosition = coinBag
			};
		}

		private List<List<Game.TileType>> ParseRows(int width, int height)
		{
			var tiles = new List<List<Game.TileType>>();
			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					if (nodeName == "row")
					{
						tiles.Add(ParseRow(width));
					}
					else
					{
						throw new Exception("Expected row element inside rows, found " + nodeName + GetCurrentLine());
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "rows")
				{
					break;
				}
			}
			

			if (tiles.Count != height)
			{
				throw new Exception("Number of rows in level does not match height");
			}

			return tiles;
		}

		private List<Game.TileType> ParseRow(int width)
		{
			var row = new List<Game.TileType>();
			var autoIndex = -1;
			Game.TileType autoTile = Game.TileType.Empty;

			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					Game.TileType type = nodeName switch
					{
						"empty" => Game.TileType.Empty,
						"ice" => Game.TileType.Ice,
						"water" => Game.TileType.Water,
						"thick" => Game.TileType.ThickIce,
						"wall" => Game.TileType.Wall,
						"goal" => Game.TileType.Goal,
						"warp" => Game.TileType.Teleporter,
						"plaid" =>  Game.TileType.PlaidTeleporter,
						"lock" => Game.TileType.Lock,
						"button" => Game.TileType.Button,
						"faketemp" => Game.TileType.FakeTemporaryWall,
						"fakepass" => Game.TileType.FakePassableWall,
						"fakeimpass" => Game.TileType.FakeImpassableWall,
						"hole" => Game.TileType.BlockHole,
						_ => throw new Exception("Invalid tile type: " + nodeName)
					};
					var attributesDict = new Dictionary<string, string>();
					for (int idx = 0; idx < GetAttributeCount(); idx++)
					{
						attributesDict[GetAttributeName(idx)] = GetAttributeValue(idx);
					}
					if (attributesDict.ContainsKey("n"))
					{
						var n = attributesDict["n"];
						if (n == "auto")
						{
							if (autoIndex == -1)
							{
								autoIndex = row.Count;
								autoTile = type;
							}
							else
							{
								throw new Exception("More than one automatic completion found in row");
							}
						}
						else
						{
							if (!int.TryParse(n, out int times))
							{
								throw new Exception("Number of times in tile is not integer");
							}
							for (int j = 0; j < times; j++)
							{
								row.Add(type);
							}
						}
					}
					else
					{
						row.Add(type);
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "row")
				{
					break;
				}
			}

			if (autoIndex != -1)
			{
				var times = width - row.Count;
				for (int k = 0; k < times; k++)
				{
					row.Insert(autoIndex, autoTile);
				}
			}

			if (row.Count != width)
			{
				throw new Exception("Row tile count does not match the row width" + GetCurrentLine());
			}

			return row;
		}

		private List<List<Game.TileType>> ParseAndApplyAlt(List<List<Game.TileType>> tileMap, Vector2I origin)
		{
			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					if (nodeName == "patch")
					{
						tileMap = ParseAndApplyPatch(tileMap, origin);
					}
					else
					{
						throw new Exception("Unexpected element inside alt, found " + nodeName + GetCurrentLine());
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "alt")
				{
					break;
				}
			}
			return tileMap;
		}

		private List<List<Game.TileType>> ParseAndApplyPatch(List<List<Game.TileType>> tileMap, Vector2I origin)
		{
			var size = -1 * Vector2I.One;
			var patchOrigin = -1 * Vector2I.One;
			var gotSize = false;
			var rows = new List<List<Game.TileType>>();
			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					var attributesDict = new Dictionary<string, string>();
					for (int idx = 0; idx < GetAttributeCount(); idx++)
					{
						attributesDict[GetAttributeName(idx)] = GetAttributeValue(idx);
					}
					switch (nodeName)
					{
						case "size":
							size = ParseSizeElement(attributesDict, size);
							if (size.X == -1 || size.Y == -1)
							{
								throw new Exception("Did not properly add size to patch");
							}
							gotSize = true;
							break;
						case "origin":
							patchOrigin = ParseGridCoordinateElement(attributesDict, patchOrigin);
							break;
						case "rows":
							if (!gotSize)
							{
								throw new Exception("Did not add size to patch");
							}
							rows = ParseRows(size.X, size.Y);
							break;
						default:
							throw new Exception("Unexpected element inside alt, found " + nodeName + GetCurrentLine());
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "patch")
				{
					break;
				}
			}

			if (patchOrigin.X == -1 || patchOrigin.Y == -1)
			{
				throw new Exception("Did not add origin to patch");
			}
			if (rows.Count == 0)
			{
				throw new Exception("Did not supply rows to patch");
			}
			if (rows.Count != size.Y)
			{
				throw new Exception("Incorrect number of rows in patch");
			}
			if (rows.Count + patchOrigin.Y - origin.Y > tileMap.Count)
			{
				throw new Exception("The patch exceeds the number of rows");
			}
			for (int y = 0; y < rows.Count; y++)
			{
				var row = rows[y];
				if (row.Count != size.X)
				{
					throw new Exception("Incorrect number of columns in patch");
				}
				if (row.Count + patchOrigin.X - origin.X > tileMap[y].Count)
				{
					throw new Exception("The patch exceeds the number of columns");
				}
				if (row.Count != size.X)
				{
					throw new Exception("Incorrect number of tiles in row for patch");
				}
				for (int x = 0; x < row.Count; x++)
				{
					GD.Print(GetCurrentLine());
					GD.Print(y + patchOrigin.Y - origin.Y);
					GD.Print("Patch origin", origin);
					GD.Print(y, patchOrigin.Y, origin.Y);
					tileMap[y + patchOrigin.Y - origin.Y][x + patchOrigin.X - origin.X] = row[x];
				}
			}

			return tileMap;
		}

		private void SkipAlt()
		{
			while (Read() != Error.FileEof)
			{
				if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == "alt")
				{
					break;
				}
			}
		}

		private List<Vector2I> ParseCoordinateList(string rootName, string elementName)
		{
			var list = new List<Vector2I>();
			while (Read() !=  Error.FileEof)
			{
				if (GetNodeType() == NodeType.Element)
				{
					var nodeName = GetNodeName();
					if (nodeName == elementName)
					{
						var attributesDict = new Dictionary<string, string>();
						for (int idx = 0; idx < GetAttributeCount(); idx++)
						{
							attributesDict[GetAttributeName(idx)] = GetAttributeValue(idx);
						}
						var coords = ParseGridCoordinateElement(attributesDict, -1 * Vector2I.One);
						if (coords.X == -1 || coords.Y == -1)
						{
							throw new Exception("Improper coordinates for " + rootName);
						}
						list.Add(coords);
					}
					else
					{
						throw new Exception($"Unexpected element inside {rootName}, found " + nodeName + GetCurrentLine());
					}
				}
				else if (GetNodeType() == NodeType.ElementEnd && GetNodeName() == rootName)
				{
					break;
				}
			}

			return list;
		}


		private static Vector2I ParseCoordinateElement(Dictionary<string, string> attributes, string keyX, string keyY, string nameX, string nameY, Vector2I min, Vector2I max, Vector2I initial)
		{
			string[] keys = new[] { keyX, keyY };
			string[] names = new[] { nameX, nameY };
			var i = 0;
			foreach(var key in keys)
			{
				if (attributes.ContainsKey(key))
				{
					var value = attributes[key];
					var success = false;
					if (i == 0)
					{
						success = int.TryParse(value, out initial.X);
					}
					else
					{
						success = int.TryParse(value, out initial.Y);
					}
					if (!success)
					{
						throw new Exception($"{names[i]} must be integer");
					}
					if (initial[i] < min[i] || initial[i] > max[i])
					{
						throw new Exception($"{names[i]} is out of range: " + initial[i]);
					}
				}
				i++;
			}

			return initial;
		}

		private static Vector2I ParseGridCoordinateElement(Dictionary<string, string> attributes, Vector2I initial)
		{
			return ParseCoordinateElement(attributes, "x", "y", "X", "Y", Vector2I.Zero, new Vector2I(Game.Level.MaxWidth - 1, Game.Level.MaxHeight - 1), initial);
		}

		private static Vector2I ParseSizeElement(Dictionary<string, string> attributes, Vector2I initial)
		{
			return ParseCoordinateElement(attributes, "w", "h", "Width", "Height", new Vector2I(1, 1), new Vector2I(Game.Level.MaxWidth, Game.Level.MaxHeight), initial);
		}
	}
}
