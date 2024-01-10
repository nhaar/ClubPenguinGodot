using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Responsible for the moving block in Thin Ice
/// </summary>
public partial class ThinIceBlock : Sprite2D
{
	/// <summary>
	/// Block coordinate
	/// </summary>
	public Vector2I Coordinates { get; set; }

	/// <summary>
	/// Reference to game instance
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Direction it's currently moving towards (if start moving)
	/// </summary>
	private ThinIcePuffle.Direction _movementDirection;

	/// <summary>
	/// The amount of frames it takes for the block to move a tile
	/// </summary>
	public static readonly int MoveAnimationDuration = 3;

	/// <summary>
	/// Whether or not the block is moving
	/// </summary>
	public bool IsMoving;

	/// <summary>
	/// Grid coordinate the block is moving from
	/// </summary>
	private Vector2I _coordinateMovingFrom;

	/// <summary>
	/// Grid coordinate the block is moving towards
	/// </summary>
	private Vector2I _coordinatesMovingTo;

	/// <summary>
	/// Position in screen space the block is moving from
	/// </summary>
	private Vector2 _positionMovingFrom;

	/// <summary>
	/// Vector for the total displacement of the block while moving from
	/// this tile to the next
	/// </summary>
	private Vector2 _displacementVector;

	/// <summary>
	/// How many frames the block has been moving for
	/// </summary>
	private int _moveAnimationTimer;

	/// <summary>
	/// Tiles the block cannot move through
	/// </summary>
	public static readonly List<ThinIceGame.TileType> ImpassableTiles = new()
	{
		ThinIceGame.TileType.Wall,
		ThinIceGame.TileType.Water,
	};

	public override void _Ready()
	{
		Game = (ThinIceGame)GetParent();
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
	public bool CanPush(ThinIcePuffle.Direction direction)
	{
		Vector2I targetCoords = ThinIcePuffle.GetDestination(Coordinates, direction);
		return !ImpassableTiles.Contains(Game.Tiles[targetCoords.X, targetCoords.Y].TileType);
	}

	/// <summary>
	/// Start moving the block in the given direction
	/// </summary>
	/// <param name="direction"></param>
	public void Move(ThinIcePuffle.Direction direction)
	{
		_movementDirection = direction;
		_coordinateMovingFrom = Coordinates;
		_coordinatesMovingTo = ThinIcePuffle.GetDestination(Coordinates, direction);
		_positionMovingFrom = Position;
		Vector2 targetPosition = Game.Tiles[_coordinatesMovingTo.X, _coordinatesMovingTo.Y].Position;
		_displacementVector = targetPosition - Position;
		StartMoveAnimation();

		Game.GetTile(Coordinates).BlockReference = null;
	}

	/// <summary>
	/// Start the move animation towards the next tile
	/// </summary>
	public void StartMoveAnimation()
	{
		_moveAnimationTimer = 0;
		IsMoving = true;
		ContinueMoveAnimation();
	}

	/// <summary>
	/// Continue the move animation towards the next tile
	/// </summary>
	public void ContinueMoveAnimation()
	{
		_moveAnimationTimer++;
		if (_moveAnimationTimer > MoveAnimationDuration)
		{
			StopMoveAnimation();
		}
		else
		{
			Position = _positionMovingFrom + _displacementVector * _moveAnimationTimer / MoveAnimationDuration;
		}
	}

	/// <summary>
	/// Stop the move animation to the next tile, continuing if it can be pushed further
	/// </summary>
	public void StopMoveAnimation()
	{
		Coordinates = _coordinatesMovingTo;
		// add block to new tile
		Game.GetTile(Coordinates).BlockReference = this;
		if (CanPush(_movementDirection))
		{
			Move(_movementDirection);
		}
		else
		{
			IsMoving = false;
		}
	}
}
