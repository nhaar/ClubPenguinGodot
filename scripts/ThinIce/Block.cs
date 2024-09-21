using Godot;
using System;
using ClubPenguinPlus.Utils;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Movable block
	/// </summary>
	public partial class Block : MovableObject
	{
		public Game Game { get; set; }

		protected override int MoveAnimationDuration => 3;

		public override void _Process(double delta)
		{
			if (IsMoving)
			{
				ContinueMoveAnimation();
			}
		}

		/// <summary>
		/// Whether or not the block can be pushed in the given direction
		/// </summary>
		public bool CanPush(Direction direction)
		{
			Vector2I targetCoords = GetAdjacentCoords(direction);
			return !ImpassableTiles.Contains(Game.GetTile(targetCoords).TileType);
		}

		/// <summary>
		/// Start moving the block in the given direction
		/// </summary>
		public void Move(Direction direction)
		{
			var targetCoords = GetAdjacentCoords(direction);
			var targetPos = Game.GetTile(targetCoords).Position;
			base.StartMoveAnimation(targetCoords, direction, targetPos);
			Game.GetTile(Coordinates).BlockReference = null;
		}

		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			// add block to new tile
			var currentTile = Game.GetTile(Coordinates);
			currentTile.BlockReference = this;

			if (currentTile.TileType == Tile.Type.Teleporter)
			{
				currentTile.BlockReference = null;
				var warpTile = currentTile.LinkedTeleporter;
				Coordinates = warpTile.TileCoordinate;
				Position = warpTile.Position;
				Move(MovementDirection);
			}
			else if (CanPush(MovementDirection))
			{
				Move(MovementDirection);
			}
		}
	}
}
