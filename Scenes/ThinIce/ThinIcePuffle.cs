using Godot;
using System;
using System.Collections.Generic;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public partial class ThinIcePuffle : Sprite2D
{
	public static readonly int MoveAnimationDuration = 4;

	public Vector2I Coordinates { get; set; }

	public bool IsMoving { get; set; }

	public int MoveAnimationTimer { get; set; }

	public ThinIceGame Game { get; set; }

	public int KeyCount { get; set; }

	private Vector2 _positionMovingFrom;

	private Vector2I _movementTargetCoords;

	private Vector2 _movementDisplacement;

	private Direction _movementDirection;

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
		None
	}

	public static readonly List<ThinIceGame.TileType> ImpassableTiles = new()
	{
		ThinIceGame.TileType.Wall,
		ThinIceGame.TileType.Water,
	};



	public override void _Ready()
	{
		Game = (ThinIceGame)GetParent();
		KeyCount = 0;
		IsMoving = false;
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
			List<Direction> pressedDirections = new();
			if (Input.IsPhysicalKeyPressed(Key.Up))
			{
				pressedDirections.Add(Direction.Up);
			}
			if (Input.IsPhysicalKeyPressed(Key.Down))
			{
				pressedDirections.Add(Direction.Down);
			}
			if (Input.IsPhysicalKeyPressed(Key.Right))
			{
				pressedDirections.Add(Direction.Right);
			}
			if (Input.IsPhysicalKeyPressed(Key.Left))
			{
				pressedDirections.Add(Direction.Left);
			}
			if (pressedDirections.Count > 0)
			{
				foreach (Direction direction in pressedDirections)
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

	public Vector2I GetDestination(Vector2I originalCoords, Direction direction)
	{
		Vector2I deltaCoord = direction switch
		{
			Direction.Up => new Vector2I(0, -1),
			Direction.Down => new Vector2I(0, 1),
			Direction.Left => new Vector2I(-1, 0),
			Direction.Right => new Vector2I(1, 0),
			_ => throw new NotImplementedException(),
		};
		return originalCoords + deltaCoord;
	}

	public bool CanMove(Vector2I targetCoords, Direction direction)
	{
		ThinIceTile targetTile = Game.Tiles[targetCoords.X, targetCoords.Y];
		
		if (ImpassableTiles.Contains(targetTile.TileType))
		{
			return false;
		}
		else if (targetTile.TileType == ThinIceGame.TileType.Lock)
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

	public void StartMoveAnimation(Vector2I targetCoords, Direction direction)
	{
		_movementTargetCoords = targetCoords;
		_movementDirection = direction;
		_positionMovingFrom = Position;
		_movementDisplacement = Game.Tiles[targetCoords.X, targetCoords.Y].Position - _positionMovingFrom;
		MoveAnimationTimer = 0;
		IsMoving = true;
		ContinueMoveAnimation();
	}

	public void ContinueMoveAnimation()
	{
		MoveAnimationTimer++;
		Position = _positionMovingFrom + _movementDisplacement * MoveAnimationTimer / MoveAnimationDuration;
		if (MoveAnimationTimer == MoveAnimationDuration)
		{
			FinishMoveAnimation();
		}
	}

	public void FinishMoveAnimation()
	{
		Game.Tiles[Coordinates.X, Coordinates.Y].OnPuffleExit();
		Coordinates = _movementTargetCoords;
		Game.Tiles[_movementTargetCoords.X, _movementTargetCoords.Y].OnPuffleEnter(_movementDirection);
		IsMoving = false;
	}

	public void GetKey()
	{
		KeyCount++;
	}

	public void UseKey()
	{
		KeyCount--;
	}

	public void TeleportTo(Vector2I coords)
	{
		Position = Game.Tiles[coords.X, coords.Y].Position;
		Coordinates = coords;
	}

	public void ResetKeys()
	{
		KeyCount = 0;
	}
}
