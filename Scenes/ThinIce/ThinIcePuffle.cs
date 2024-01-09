using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Object for the playable puffle
/// </summary>
public partial class ThinIcePuffle : Sprite2D
{
	/// <summary>
	/// Amount of time it takes for the puffle to move a tile
	/// </summary>
	public static readonly int MoveAnimationDuration = 4;

	/// <summary>
	/// Grid coordinates where the puffle is locaed
	/// </summary>
	public Vector2I Coordinates { get; set; }

	/// <summary>
	/// Whether or not the puffle is moving
	/// </summary>
	public bool IsMoving { get; set; }

	/// <summary>
	/// Counter for the move animation
	/// </summary>
	public int MoveAnimationTimer { get; set; }

	/// <summary>
	/// Reference to the game object
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Number of keys the puffle has
	/// </summary>
	/// <remarks>
	/// In vanilla, each level has at most one key. This is thus meant for custom levels.
	/// </remarks>
	public int KeyCount { get; set; }

	/// <summary>
	/// Position in screen space the puffle is moving from
	/// </summary>
	private Vector2 _positionMovingFrom;

	/// <summary>
	/// Coordinate pair the puffle is moving towards
	/// </summary>
	private Vector2I _movementTargetCoords;

	/// <summary>
	/// Displacement vector for the puffle's movement towards the next tile
	/// </summary>
	private Vector2 _movementDisplacement;

	/// <summary>
	/// Direction the puffle is moving towards
	/// </summary>
	private Direction _movementDirection;

	/// <summary>
	/// Represents a direction the puffle can move in
	/// </summary>
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right,
	}

	/// <summary>
	/// All tiles the puffle cannot move through
	/// </summary>
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

	/// <summary>
	/// Gets the coordinate from moving in the given direction from the original coordinate
	/// </summary>
	/// <param name="originalCoords"></param>
	/// <param name="direction"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public static Vector2I GetDestination(Vector2I originalCoords, Direction direction)
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

	/// <summary>
	/// Whether or not the puffle can move in a given direction
	/// </summary>
	/// <param name="targetCoords"></param>
	/// <param name="direction"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Starts moving towards a tile
	/// </summary>
	/// <param name="targetCoords"></param>
	/// <param name="direction"></param>
	public void StartMoveAnimation(Vector2I targetCoords, Direction direction)
	{
		Game.Tiles[Coordinates.X, Coordinates.Y].OnPuffleExit(direction);
		_movementTargetCoords = targetCoords;
		_movementDirection = direction;
		_positionMovingFrom = Position;
		_movementDisplacement = Game.Tiles[targetCoords.X, targetCoords.Y].Position - _positionMovingFrom;
		MoveAnimationTimer = 0;
		IsMoving = true;
		ContinueMoveAnimation();
	}

	/// <summary>
	/// Progresses animation towards the next tile
	/// </summary>
	public void ContinueMoveAnimation()
	{
		MoveAnimationTimer++;
		Position = _positionMovingFrom + _movementDisplacement * MoveAnimationTimer / MoveAnimationDuration;
		if (MoveAnimationTimer == MoveAnimationDuration)
		{
			FinishMoveAnimation();
		}
	}

	/// <summary>
	/// Finishes moving towards the next tile
	/// </summary>
	public void FinishMoveAnimation()
	{
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
}
