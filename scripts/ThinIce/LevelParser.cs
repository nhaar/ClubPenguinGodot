using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	public partial class LevelParser
	{
		private static readonly string MapKey = "m";

		private static readonly string CoinBagKey = "c";

		private static readonly string BlocksKey = "b";

		private static readonly string AltKey = "a";

		private static readonly string SpawnKey = "s";

		private static readonly string OriginKey = "o";

		private static readonly string KeysKey = "k";

		private int? CurrentLevel { get; set; } = null;

		public Game.Level[] ParseLevels(string file)
		{
			try
			{
				var levels = new List<Game.Level>();
				var jsonString = FileAccess.Open(file, FileAccess.ModeFlags.Read).GetAsText();
				var jsonParser = new Json();
				if (jsonParser.Parse(jsonString) == Error.Ok)
				{
					var data = jsonParser.Data;
					if (data.VariantType == Variant.Type.Array)
					{
						CurrentLevel = 0;
						var levelsArray = data.AsGodotArray();
						foreach (var levelRepresentation in levelsArray)
						{
							CurrentLevel++;
							if (levelRepresentation.VariantType == Variant.Type.Dictionary)
							{
								var levelData = levelRepresentation.AsGodotDictionary<string, Variant>();
								levels.Add(ParseLevelData(levelData));
							}
							else
							{
								throw new Exception($"Level was represented with {levelRepresentation.VariantType} instead of using a dictionary");
							}
						}
					}
					else
					{
						throw new Exception($"Expected an array of levels at the root of levels file, instead found {data.VariantType}");
					}
				}
				else
				{
					throw new Exception($"Failed to parse JSON: {jsonParser.GetErrorMessage()}\nLine {jsonParser.GetErrorLine()}");
				}

				return levels.ToArray();
			}
			catch (Exception ex)
			{
				var levelMessage = CurrentLevel == null ? "" : $"for level {CurrentLevel}";
				throw new Exception($"Error while parsing Thin Ice level file {levelMessage}: {ex.Message}\n{ex.StackTrace}");
			}
		}

		private static Game.Level ParseLevelData(Godot.Collections.Dictionary<string, Variant> levelData)
		{
			var origin = Vector2I.Zero;
			Vector2I spawn;

			var keys = new List<Vector2I>();
			var blocks = new List<Vector2I>();
			Vector2I? bag = null;
			List<List< Game.TileType >> map;


			Vector2I size;
			var maxSize = new Vector2I(Game.Level.MaxWidth, Game.Level.MaxHeight);
			if (levelData.ContainsKey(MapKey))
			{
				try
				{
					TryParseMap(levelData[MapKey], out map, out size);
				}
				catch (Exception ex)
				{
					throw new Exception($"Failed parsing level map: {ex.Message}\n{ex.StackTrace}");
				}
			}
			else
			{
				throw new Exception("Map for level was not given");
			}
			if (size.X > maxSize.X || size.Y > maxSize.Y)
			{
				throw new Exception("Map given is bigger than the maximum allowed");
			}

			if (levelData.ContainsKey(OriginKey))
			{
				try
				{
					TryGetCoordinate(levelData[OriginKey], out origin, Vector2I.Zero, maxSize - size);
				}
				catch (Exception ex)
				{
					throw new Exception($"Could not parse origin coordinate in level: {ex.Message}\n{ex.StackTrace}");
				}
			}

			var maxCoord = size + origin;
			if (levelData.ContainsKey(SpawnKey))
			{
				try
				{
					TryGetCoordinate(levelData[SpawnKey], out spawn, origin, maxCoord);
				}
				catch (Exception ex)
				{
					throw new Exception($"Could not parse spawn coordinate in level: {ex.Message}\n{ex.StackTrace}");
				}
			}
			else
			{
				throw new Exception("Spawn for level was not given");
			}
			if (levelData.ContainsKey(CoinBagKey))
			{
				try
				{
					TryGetCoordinate(levelData[CoinBagKey], out var bagOut, origin, maxCoord);
					bag = bagOut;
				}
				catch (Exception ex)
				{
					throw new Exception($"Could not parse spawn coordinate in level: {ex.Message}\n{ex.StackTrace}");
				}
			}
			if (levelData.ContainsKey(KeysKey))
			{
				try
				{
					TryGetCoordinateList(levelData[KeysKey], out keys, origin, maxCoord);
				}
				catch (Exception ex)
				{
					throw new Exception($"Could not parse key coordinates: {ex.Message}\n{ex.StackTrace}");
				}
			}
			if (levelData.ContainsKey(BlocksKey))
			{
				try
				{
					TryGetCoordinateList(levelData[BlocksKey], out blocks, origin, maxCoord);
				}
				catch (Exception ex)
				{
					throw new Exception($"Could not parse block coordinates: {ex.Message}\n{ex.StackTrace}");
				}
			}

			// 50/50 as per the original game of having alternate
			var useAlt = GD.Randf() > 0.5;

			// applying patches for alternate level
			if (useAlt && levelData.ContainsKey(AltKey))
			{
				var altValue = levelData[AltKey];
				if (altValue.VariantType == Variant.Type.Array)
				{
					var patchesArray = altValue.AsGodotArray();
					foreach (var patch in patchesArray)
					{
						if (patch.VariantType == Variant.Type.Dictionary)
						{
							var patchValue = patch.AsGodotDictionary<string, Variant>();
							if (!patchValue.ContainsKey(MapKey))
							{
								throw new Exception("Patch is expected to have a map");
							}
							Vector2I patchSize;
							Vector2I patchOrigin;
							List<List<Game.TileType>> patchMap;
							try
							{
								TryParseMap(patchValue[MapKey], out patchMap, out patchSize);
							}
							catch (Exception ex)
							{
								throw new Exception($"Could not parse patch map: {ex.Message}\n{ex.StackTrace}");
							}
							if (!patchValue.ContainsKey(OriginKey))
							{
								throw new Exception("Patch is expected to have a origin");
							}
							try
							{
								TryGetCoordinate(patchValue[OriginKey], out patchOrigin, origin, maxCoord - patchSize);
							}
							catch (Exception ex)
							{
								throw new Exception($"Could not parse origin for patch: {ex.Message}\n{ex.StackTrace}");
							}

							for (int y = 0; y < patchMap.Count; y++)
							{
								var patchRow = patchMap[y];
								for (int x = 0; x < patchRow.Count; x++)
								{
									map[y + patchOrigin.Y - origin.Y][x + patchOrigin.X - origin.X] = patchRow[x];
								}
							}
						}
						else
						{
							throw new Exception("Patch in level JSON was not of type dictionary");
						}
					}
				}
				else
				{
					throw new Exception("Alt property in level JSON is not an array");
				}
			}

			var tiles = new Game.TileType[size.X, size.Y];
			for (int i = 0; i < map.Count; i++)
			{
				for (int j = 0; j < map[i].Count; j++)
				{
					try
					{
						tiles[j, i] = map[i][j];
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
				CoinBagPosition = bag
			};
		}
	
		private static void TryGetCoordinate(Variant value, out Vector2I coordinates, Vector2I min, Vector2I max)
		{
			coordinates = Vector2I.Zero;
			if (value.VariantType == Variant.Type.Array)
			{
				var valueArray = value.AsGodotArray();
				var coordList = new List<int>();
				foreach (var element in valueArray)
				{
					if (element.VariantType == Variant.Type.Float)
					{
						coordList.Add(element.AsInt32());
					}
					else
					{
						throw new Exception($"Coordinate member was not a number, but a {element.VariantType}: {element}");
					}
				}

				if (coordList.Count == 2)
				{
					var x = coordList[0];
					var y = coordList[1];
					if (x < min.X || y < min.Y || x > max.X|| y > max.Y)
					{
						throw new Exception($"Coordinate pair ({x},{y}) is not within bounds of map");
					}
					coordinates = new Vector2I(x, y);
				}
				else
				{
					throw new Exception($"Coordinate pair did not have two elements: {coordList}");
				}
			}
			else
			{
				throw new Exception($"Coordinate list was not an array in JSON: {value}");
			}
		}

		private static void TryGetCoordinateList(Variant value, out List<Vector2I> coordList, Vector2I min, Vector2I max)
		{
			coordList = new List<Vector2I>();
			if (value.VariantType == Variant.Type.Array)
			{
				var valueArray = value.AsGodotArray();
				foreach (var element in valueArray)
				{
					Vector2I coord;
					try
					{
						TryGetCoordinate(element, out coord, min, max);
					}
					catch (Exception ex)
					{
						throw new Exception($"Could not get coordinate inside coordinate list: {ex.Message}\n{ex.StackTrace}");
					}
					coordList.Add(coord);
				}
			}
			else
			{
				throw new Exception($"Coordinate list ({value}) was supposed to be an array, found {value.VariantType}");
			}
		}

		private static void TryParseMap(Variant value, out List<List<Game.TileType>> map, out Vector2I size)
		{
			map = new List<List<Game.TileType>>();
			size = Vector2I.Zero;
			var width = -1;
			if (value.VariantType == Variant.Type.Array)
			{
				var mapGrid = value.AsGodotArray();

				foreach (var row in mapGrid)
				{
					var rowList = new List<Game.TileType>();
					if (row.VariantType == Variant.Type.Array)
					{
						var rowValue = row.AsGodotArray();
						if (width == -1)
						{
							width = rowValue.Count;
						}
						else if (rowValue.Count != width)
						{
							throw new Exception($"Rows for map aren't all of the same length");
						}
						foreach (var tile in rowValue)
						{
							if (tile.VariantType == Variant.Type.Float)
							{
								var tileValue = tile.AsInt32();
								var type = tileValue switch
								{
									0 => Game.TileType.Empty,
									1 => Game.TileType.Ice,
									2 => Game.TileType.ThickIce,
									3 => Game.TileType.Wall,
									4 => Game.TileType.Goal,
									5 => Game.TileType.Teleporter,
									6 => Game.TileType.Lock,
									7 => Game.TileType.Button,
									8 => Game.TileType.FakeTemporaryWall,
									9 => Game.TileType.FakePassableWall,
									10 => Game.TileType.FakeImpassableWall,
									11 => Game.TileType.BlockHole,
									12 => Game.TileType.PlaidTeleporter,
									13 => Game.TileType.Water,
									_ => throw new Exception($"{tileValue} is not a valid value for a tile")
								} ;
								rowList.Add(type);

							}
							else
							{
								throw new Exception("Tile was not a number");
							}
						}
						map.Add(rowList);
					}
					else
					{
						throw new Exception("Row inside map was not an array");
					}
				}

				size = new Vector2I(map[0].Count, map.Count);
			}
			else
			{
				throw new Exception("Map was not an array");
			}
		}
	}
}
