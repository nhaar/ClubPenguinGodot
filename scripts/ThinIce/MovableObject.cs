using Godot;
using System;
using System.Collections.Generic;
using ClubPenguinPlus.Utils;

namespace ClubPenguinPlus.ThinIce
{
    /// <summary>
    /// Base for an object that moves across each tile at a fixed rate
    /// </summary>
    public abstract partial class MovableObject : Sprite2D
    {
        /// <summary>
        /// Coordinates the object is in the grid
        /// </summary>
        public Vector2I Coordinates { get; set; }

        /// <summary>
        /// Direction of the ongoing movement (if moving)
        /// </summary>
        protected Direction MovementDirection { get; set; }

        /// <summary>
        /// How many frames it takes for this object to move across a tile
        /// </summary>
        protected abstract int MoveAnimationDuration { get; }

        protected bool IsMoving { get; set; } = false;

        /// <summary>
        /// How many frames the current movement has lasted
        /// </summary>
        private int MoveAnimationTimer { get; set; } = 0;

        /// <summary>
        /// Position the object is coming from in current movement
        /// </summary>
        private Vector2 PositionMovingFrom { get; set; }

        /// <summary>
        /// Coordinate pair the object is moving towards
        /// </summary>
        private Vector2I MovementTargetCoords { get; set; }

        /// <summary>
        /// Displacement vector for the objects's movement towards the next tile
        /// </summary>
        private Vector2 MovementDisplacement { get; set; }

        /// <summary>
        /// All tiles in which objects can't move through
        /// </summary>
        public static readonly List<Tile.Type> ImpassableTiles = new()
        {
            Tile.Type.Wall,
            Tile.Type.Water,
            Tile.Type.FakeImpassableWall,
        };

        /// <summary>
        /// Gets the coordinate from moving in the given direction from the original coordinate
        /// </summary>
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

        protected virtual void StartMoveAnimation(Vector2I targetCoords, Direction direction, Vector2 targetPos)
        {
            MovementTargetCoords = targetCoords;
            MovementDirection = direction;
            PositionMovingFrom = Position;
            MovementDisplacement = targetPos - PositionMovingFrom;
            MoveAnimationTimer = 0;
            IsMoving = true;
            ContinueMoveAnimation();
        }

        protected void ContinueMoveAnimation()
        {
            MoveAnimationTimer++;
            Position = PositionMovingFrom + MovementDisplacement * MoveAnimationTimer / MoveAnimationDuration;
            if (MoveAnimationTimer == MoveAnimationDuration)
            {
                FinishMoveAnimation();
            }
        }

        protected virtual void FinishMoveAnimation()
        {
            Coordinates = MovementTargetCoords;
            IsMoving = false;
        }

        protected Vector2I GetAdjacentCoords(Direction direction)
        {
            return GetDestination(Coordinates, direction);
        }

    }
}
