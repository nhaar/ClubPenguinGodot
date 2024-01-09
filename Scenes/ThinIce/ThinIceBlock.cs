using Godot;
using System;
using System.Collections.Generic;

public partial class ThinIceBlock : Sprite2D
{
	public Vector2I Coordinates { get; set; }

	public ThinIceGame Game { get; set; }

	private ThinIcePuffle.Direction _movementDirection;

	public static readonly int MoveAnimationDuration = 3;

	public bool IsMoving;

	private Vector2I _coordinateMovingFrom;

	private Vector2I _coordinatesMovingTo;

	private Vector2 _positionMovingFrom;

	private Vector2 _displacementVector;

	private int _moveAnimationTimer;

	public static readonly List<ThinIceGame.TileType> ImpassableTiles = new()
	{
		ThinIceGame.TileType.Wall,
		ThinIceGame.TileType.Water,
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game = (ThinIceGame)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (IsMoving)
		{
            ContinueMoveAnimation();
        }
	}

	public bool CanPush(ThinIcePuffle.Direction direction)
	{
		Vector2I targetCoords = Game.Puffle.GetDestination(Coordinates, direction);
		return !ImpassableTiles.Contains(Game.Tiles[targetCoords.X, targetCoords.Y].TileType);
	}

	public void Move(ThinIcePuffle.Direction direction)
	{
		_movementDirection = direction;
		_coordinateMovingFrom = Coordinates;
		_coordinatesMovingTo = Game.Puffle.GetDestination(Coordinates, direction);
		_positionMovingFrom = Position;
		Vector2 targetPosition = Game.Tiles[_coordinatesMovingTo.X, _coordinatesMovingTo.Y].Position;
		_displacementVector = targetPosition - Position;
		StartMoveAnimation();
	}

	public void StartMoveAnimation()
	{
		_moveAnimationTimer = 0;
		IsMoving = true;
		ContinueMoveAnimation();
	}

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

	public void StopMoveAnimation()
	{
		Coordinates = _coordinatesMovingTo;
		if (CanPush(_movementDirection))
		{
			Move(_movementDirection);
		}
	}
}
