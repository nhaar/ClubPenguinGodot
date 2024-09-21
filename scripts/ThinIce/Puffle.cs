using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Object for the playable puffle
	/// </summary>
	public partial class Puffle : MovableObject
	{
		/// <summary>
		/// Amount of time it takes for the puffle to move a tile
		/// </summary>
		protected override int MoveAnimationDuration => 4;

		/// <summary>
		/// Reference to the game object
		/// </summary>
		public Game Game { get; set; }

		/// <summary>
		/// Number of keys the puffle has
		/// </summary>
		/// <remarks>
		/// In vanilla, each level has at most one key. This is thus meant for custom levels.
		/// </remarks>
		public int KeyCount { get; set; }

		public override void _Ready()
		{
			base._Ready();
			Game = (Game)GetParent();
			KeyCount = 0;
		}

		public override void _Process(double delta)
		{
			if (IsMoving)
			{
				ContinueMoveAnimation();
			}
			else
			{
				// preserving the original code's arrow key priority
				List<Game.Direction> pressedDirections = new();
				if (Input.IsPhysicalKeyPressed(Godot.Key.Up))
				{
					pressedDirections.Add(Game.Direction.Up);
				}
				if (Input.IsPhysicalKeyPressed(Godot.Key.Down))
				{
					pressedDirections.Add(Game.Direction.Down);
				}
				if (Input.IsPhysicalKeyPressed(Godot.Key.Left))
				{
					pressedDirections.Add(Game.Direction.Left);
				}
				if (Input.IsPhysicalKeyPressed(Godot.Key.Right))
				{
					pressedDirections.Add(Game.Direction.Right);
				}
				if (pressedDirections.Count > 0)
				{
					foreach (var direction in pressedDirections)
					{
						Vector2I targetCoords = GetDestination(Coordinates, direction);
						if (CanMove(targetCoords, direction))
						{
							StartMoveAnimation(targetCoords, direction);
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// Whether or not the puffle can move in a given direction
		/// </summary>
		/// <param name="targetCoords"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public bool CanMove(Vector2I targetCoords, Game.Direction direction)
		{
			Tile targetTile = Game.Tiles[targetCoords.X, targetCoords.Y];

			if (ImpassableTiles.Contains(targetTile.TileType))
			{
				return false;
			}
			else if (targetTile.TileType == Game.TileType.Lock)
			{
				return KeyCount > 0;
			}
			else if (targetTile.BlockReference != null)
			{
				return targetTile.BlockReference.CanPush(direction);
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Starts moving towards a tile
		/// </summary>
		/// <param name="targetCoords"></param>
		/// <param name="direction"></param>
		private void StartMoveAnimation(Vector2I targetCoords, Game.Direction direction)
		{
			Game.Tiles[Coordinates.X, Coordinates.Y].OnPuffleExit(direction);
			var targetPos = Game.Tiles[targetCoords.X, targetCoords.Y].Position;
			base.StartMoveAnimation(targetCoords, direction, targetPos);
		}

		/// <summary>
		/// Finishes moving towards the next tile
		/// </summary>
		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			Game.Tiles[Coordinates.X, Coordinates.Y].OnPuffleEnter();
		}

		public void GetKey()
		{
			KeyCount++;
		}

		public void UseKey()
		{
			KeyCount--;
		}
		public void ResetKeys()
		{
			KeyCount = 0;
		}

		/// <summary>
		/// Teleports the puffle to the given grid coordinates
		/// </summary>
		/// <param name="coords"></param>
		public void TeleportTo(Vector2I coords)
		{
			Position = Game.Tiles[coords.X, coords.Y].Position;
			Coordinates = coords;
		}

		/// <summary>
		/// Whether or not the puffle has anywhere to go
		/// </summary>
		/// <returns></returns>
		public bool IsStuck()
		{
			foreach (Game.Direction direction in Enum.GetValues(typeof(Game.Direction)))
			{
				if (CanMove(GetDestination(Coordinates, direction), direction))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Reset Puffle variables for when the puffle "dies"
		/// </summary>
		public void Die()
		{
			IsMoving = false;
			ResetKeys();
		}
	}
}
