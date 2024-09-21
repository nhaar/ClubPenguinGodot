using Godot;
using System;
using System.Collections.Generic;
using ClubPenguinPlus.Utils;

namespace ClubPenguinPlus.ThinIce
{
    public abstract partial class MovableObject : Sprite2D
    {
        /// <summary>
        /// Grid coordinates where the puffle is locaed
        /// </summary>
        public Vector2I Coordinates { get; set; }

        protected Direction MovementDirection { get; set; }

        protected abstract int MoveAnimationDuration { get; }

        protected bool IsMoving { get; set; } = false;

        private int MoveAnimationTimer { get; set; } = 0;

        /// <summary>
        /// Position in screen space the puffle is moving from
        /// </summary>
        private Vector2 PositionMovingFrom { get; set; }

        /// <summary>
        /// Coordinate pair the puffle is moving towards
        /// </summary>
        private Vector2I MovementTargetCoords { get; set; }

        /// <summary>
        /// Displacement vector for the puffle's movement towards the next tile
        /// </summary>
        private Vector2 MovementDisplacement { get; set; }

        public static readonly List<Tile.Type> ImpassableTiles = new()
        {
            Tile.Type.Wall,
            Tile.Type.Water,
            Tile.Type.FakeImpassableWall,
        };

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

        /// <summary>
        /// Progresses animation towards the next tile
        /// </summary>
        protected void ContinueMoveAnimation()
        {
            MoveAnimationTimer++;
            Position = PositionMovingFrom + MovementDisplacement * MoveAnimationTimer / MoveAnimationDuration;
            if (MoveAnimationTimer == MoveAnimationDuration)
            {
                FinishMoveAnimation();
            }
        }

        /// <summary>
        /// Finishes moving towards the next tile
        /// </summary>
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
