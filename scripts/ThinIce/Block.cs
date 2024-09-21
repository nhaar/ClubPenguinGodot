using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Movable block
	/// </summary>
	public partial class Block : MovableObject
	{ 
		/// <summary>
		/// Reference to game instance
		/// </summary>
		public Game Game { get; set; }

		/// <summary>
		/// The amount of frames it takes for the block to move a tile
		/// </summary>
		protected override int MoveAnimationDuration => 3;

		public override void _Ready()
		{
			Game = (Game)GetParent();
		}

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
		/// <param name="direction"></param>
		/// <returns></returns>
		public bool CanPush(Game.Direction direction)
		{
			Vector2I targetCoords = Puffle.GetDestination(Coordinates, direction);
			return !ImpassableTiles.Contains(Game.Tiles[targetCoords.X, targetCoords.Y].TileType);
		}

		/// <summary>
		/// Start moving the block in the given direction
		/// </summary>
		/// <param name="direction"></param>
		public void Move(Game.Direction direction)
		{
			var targetCoords = GetDestination(Coordinates, direction);

			var targetPos = Game.GetTile(targetCoords).Position;
			base.StartMoveAnimation(targetCoords, direction, targetPos);
			Game.GetTile(Coordinates).BlockReference = null;
		}


		/// <summary>
		/// Stop the move animation to the next tile, continuing if it can be pushed further
		/// </summary>
		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			// add block to new tile
			Tile currentTile = Game.GetTile(Coordinates);
			currentTile.BlockReference = this;

			if (currentTile.TileType == Game.TileType.Teleporter)
			{
				currentTile.BlockReference = null;
				Coordinates = currentTile.LinkedTeleporter.TileCoordinate;
				Position = Game.GetTile(Coordinates).Position;
				Move(MovementDirection);
			}
			else if (CanPush(MovementDirection))
			{
				Move(MovementDirection);
			}
		}
	}
}
