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
		public Engine Engine { get; set; }

		protected override int MoveAnimationDuration => 3;

		private bool HasMomentum { get; set; } = false;

		public override void _Process(double delta)
		{
			if (IsMoving)
			{
				ContinueMoveAnimation();
			}
			else if (HasMomentum)
			{
				if (CanPush(MovementDirection))
				{
					Move(MovementDirection);
				}
				HasMomentum = false;
			}
		}

		/// <summary>
		/// Whether or not the block can be pushed in the given direction
		/// </summary>
		public bool CanPush(Direction direction)
		{
			var targetCoords = GetAdjacentCoords(direction);
			return !ImpassableTiles.Contains(Engine.GetTile(targetCoords).TileType);
		}

		/// <summary>
		/// Start moving the block in the given direction
		/// </summary>
		public void Move(Direction direction)
		{
			var targetCoords = GetAdjacentCoords(direction);
			var targetPos = Engine.GetTile(targetCoords).Position;
			base.StartMoveAnimation(targetCoords, direction, targetPos);
			Engine.GetTile(Coordinates).BlockReference = null;
		}

		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			// add block to new tile
			var currentTile = Engine.GetTile(Coordinates);
			currentTile.BlockReference = this;

			if (currentTile.TileType == Tile.Type.Teleporter)
			{
				currentTile.BlockReference = null;
				var warpTile = currentTile.LinkedTeleporter;
				Coordinates = warpTile.TileCoordinate;
				Position = warpTile.Position;
			}
			HasMomentum = true;
		}
	}
}
