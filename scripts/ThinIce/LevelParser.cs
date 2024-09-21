using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Parser for Thin Ice levels following a custom JSON format
	/// </summary>
	internal class LevelParser
	{
		/// <summary>
		/// Key that stores map grids
		/// </summary>
		private static readonly string MapKey = "m";

		/// <summary>
		/// Key that store coin bag coordinates
		/// </summary>
		private static readonly string CoinBagKey = "c";

		/// <summary>
		/// Key that stores a list of block coordinates
		/// </summary>
		private static readonly string BlocksKey = "b";

		/// <summary>
		/// Key that stores alternate level information (random parallel version)
		/// </summary>
		private static readonly string AltKey = "a";

		/// <summary>
		/// Key that stores the spawn coordinate
		/// </summary>
		private static readonly string SpawnKey = "s";

		/// <summary>
		/// Key that stores an origin coordinate
		/// </summary>
		private static readonly string OriginKey = "o";

		/// <summary>
		/// Key that stores a list of coordinates with coordinates to in-game key objects
		/// </summary>
		private static readonly string KeysKey = "k";

		/// <summary>
		/// Current level during parsing, null if have not started any level
		/// </summary>
		private int? CurrentLevel { get; set; } = null;

		/// <summary>
		/// Parse a JSON resource containing the levels
		/// </summary>
		public Level[] ParseLevels(Json json)
		{
			try
			{ 
				var levels = new List<Level>();
				var data = json.Data;
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

				return levels.ToArray();
			}
			catch (Exception ex)
			{
				var levelMessage = CurrentLevel == null ? "" : $" for level {CurrentLevel}";
				throw new Exception($"Error while parsing Thin Ice level file{levelMessage}: {ex.Message}\n{ex.StackTrace}");
			}
		}

		/// <summary>
		/// Parse the dictionary representation of a level json
		/// </summary>
		private static Level ParseLevelData(Godot.Collections.Dictionary<string, Variant> levelData)
		{
			var origin = Vector2I.Zero;
			Vector2I spawn;

			var keys = new List<Vector2I>();
			var blocks = new List<Vector2I>();
			Vector2I? bag = null;
			List<List< Tile.Type >> map;

			Vector2I size;
			var maxSize = new Vector2I(Level.MaxWidth, Level.MaxHeight);
			if (levelData.ContainsKey(MapKey))
			{
				try
				{
					ParseMap(levelData[MapKey], out map, out size);
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
					ParseCoordinate(levelData[OriginKey], out origin, Vector2I.Zero, maxSize - size);
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
					ParseCoordinate(levelData[SpawnKey], out spawn, origin, maxCoord);
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
					ParseCoordinate(levelData[CoinBagKey], out var bagOut, origin, maxCoord);
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
					ParseCoordinateList(levelData[KeysKey], out keys, origin, maxCoord);
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
					ParseCoordinateList(levelData[BlocksKey], out blocks, origin, maxCoord);
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
							List<List<Tile.Type>> patchMap;
							try
							{
								ParseMap(patchValue[MapKey], out patchMap, out patchSize);
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
								ParseCoordinate(patchValue[OriginKey], out patchOrigin, origin, maxCoord - patchSize);
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

			var tiles = new Tile.Type[size.X, size.Y];
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

			return new Level
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
	
		/// <summary>
		/// Parse the value which should represent a coordinate (an array with two numbers in JSON)
		/// </summary>
		/// <param name="value">Coordinate from the JSON</param>
		/// <param name="coordinates">Variable the coordinates will be saved to</param>
		/// <param name="min">Minimum allowed value for coordinates</param>
		/// <param name="max">Maximum allowed value for coordinates</param>
		private static void ParseCoordinate(Variant value, out Vector2I coordinates, Vector2I min, Vector2I max)
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

		/// <summary>
		/// Parse a list of coordinates
		/// </summary>
		/// <param name="value">Value from the JSON that represents the list</param>
		/// <param name="coordList">Variable to save the list to</param>
		/// <param name="min">Minimum allowed coordinate</param>
		/// <param name="max">Maximum allowed coordinate</param>
		private static void ParseCoordinateList(Variant value, out List<Vector2I> coordList, Vector2I min, Vector2I max)
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
						ParseCoordinate(element, out coord, min, max);
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

		/// <summary>
		/// Parse a map JSON object
		/// </summary>
		/// <param name="value">Value from the JSON of the map</param>
		/// <param name="map">Variable to save the map to</param>
		/// <param name="size">Variable to save the size of the map to</param>
		private static void ParseMap(Variant value, out List<List<Tile.Type>> map, out Vector2I size)
		{
			map = new List<List<Tile.Type>>();
			size = Vector2I.Zero;
			var width = -1;
			if (value.VariantType == Variant.Type.Array)
			{
				var mapGrid = value.AsGodotArray();

				foreach (var row in mapGrid)
				{
					var rowList = new List<Tile.Type>();
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
									0 => Tile.Type.Empty,
									1 => Tile.Type.Ice,
									2 => Tile.Type.ThickIce,
									3 => Tile.Type.Wall,
									4 => Tile.Type.Goal,
									5 => Tile.Type.Teleporter,
									6 => Tile.Type.Lock,
									7 => Tile.Type.Button,
									8 => Tile.Type.FakeTemporaryWall,
									9 => Tile.Type.FakePassableWall,
									10 => Tile.Type.FakeImpassableWall,
									11 => Tile.Type.BlockHole,
									12 => Tile.Type.PlaidTeleporter,
									13 => Tile.Type.Water,
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
