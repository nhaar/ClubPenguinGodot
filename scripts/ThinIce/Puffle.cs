using Godot;
using System;
using System.Collections.Generic;
using ClubPenguinPlus.Utils;

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
		public Engine Game { get; set; }

		/// <summary>
		/// Number of keys the puffle has
		/// </summary>
		/// <remarks>
		/// In vanilla, each level has at most one key. This is thus meant for custom levels.
		/// </remarks>
		private int KeyCount { get; set; }

		private bool IsActive { get; set; }

		public override void _Ready()
		{
			base._Ready();
			KeyCount = 0;
			IsActive = false;
		}

		public void Activate()
		{
			IsActive = true;
		}

		public override void _Process(double delta)
		{
			if (!IsActive)
			{
				return;
			}
			if (IsMoving)
			{
				ContinueMoveAnimation();
			}
			else
			{
				// preserving the original code's arrow key priority
				List<Direction> pressedDirections = new();

				var inputMap = new Dictionary<Godot.Key, Direction>
				{
					{ Godot.Key.Up, Direction.Up },
					{ Godot.Key.Down, Direction.Down },
					{ Godot.Key.Left, Direction.Left },
					{ Godot.Key.Right, Direction.Right }
				};
				foreach (var entry in inputMap)
				{
					if (Input.IsPhysicalKeyPressed(entry.Key))
					{
						pressedDirections.Add(entry.Value);
					}
				}
				foreach (var direction in pressedDirections)
				{
					if (CanMove(direction))
					{
						StartMoveAnimation(direction);
						return;
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
		public bool CanMove(Direction direction)
		{
			var targetCoords = GetDestination(Coordinates, direction);
			Tile targetTile = Game.GetTile(targetCoords);

			if (ImpassableTiles.Contains(targetTile.TileType))
			{
				return false;
			}
			else if (targetTile.TileType == Tile.Type.Lock)
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
		private void StartMoveAnimation(Direction direction)
		{
			var targetCoords = GetDestination(Coordinates, direction);
			var targetTile = Game.GetTile(targetCoords);
			Game.GetTile(Coordinates).OnPuffleExit();
			targetTile.OnPuffleStartEnter(direction);
			base.StartMoveAnimation(targetCoords, direction, targetTile.Position);
		}

		/// <summary>
		/// Finishes moving towards the next tile
		/// </summary>
		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			Game.GetTile(Coordinates).OnPuffleFinishEnter(this);
		}

		public void GetKey()
		{
			KeyCount++;
		}

		public void UseKey()
		{
			KeyCount--;
		}
		private void ResetKeys()
		{
			KeyCount = 0;
		}

		/// <summary>
		/// Teleports the puffle to the given grid coordinates
		/// </summary>
		/// <param name="coords"></param>
		public void TeleportTo(Vector2I coords)
		{
			Position = Game.GetTile(coords).Position;
			Coordinates = coords;
		}

		/// <summary>
		/// Whether or not the puffle has anywhere to go
		/// </summary>
		/// <returns></returns>
		public bool IsStuck()
		{
			foreach (Direction direction in Enum.GetValues(typeof(Direction)))
			{
				if (CanMove(direction))
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
