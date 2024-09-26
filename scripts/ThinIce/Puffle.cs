using Godot;
using System;
using System.Collections.Generic;
using ClubPenguinPlus.Utils;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// The puffle protagonist
	/// </summary>
	public partial class Puffle : MovableObject
	{
		[Export]
		private SpriteFrames IdleAnimationFrames { get; set; }

		[Export]
		private SpriteFrames IgniteAnimationFrames { get; set; }

		[Export]
		private SpriteFrames SinkingAnimationFrames { get; set; }

		[Export]
		private Vector2 SinkingCorrection { get; set; }

		protected override int MoveAnimationDuration => 4;

		public Engine Engine { get; set; }

		/// <summary>
		/// Number of keys the puffle has
		/// </summary>
		/// <remarks>
		/// In vanilla, each level has at most one key. This is thus meant for custom levels.
		/// </remarks>
		private int KeyCount { get; set; }

		/// <summary>
		/// If the puffle has functionality
		/// </summary>
		private bool IsActive { get; set; }

		private bool IsIgniting { get; set; } = false;

		private bool IsIdle { get; set; } = false;

		private bool IsSinking { get; set; } = false;

		private FramerateBoundAnimation IgniteAnimation { get; set; }

		private FramerateBoundAnimation IdleAnimation { get; set; }

		private FramerateBoundAnimation SinkingAnimation { get; set; }

		/// <summary>
		/// How much to displace for the sinking animation to be centered
		/// </summary>
		private Vector2 SinkingDelta { get; set; }

		public override void _Ready()
		{
			base._Ready();
			KeyCount = 0;
			IsActive = false;
			IgniteAnimation = new(IgniteAnimationFrames, this);
			IdleAnimation = new(IdleAnimationFrames, this);
			SinkingAnimation = new(SinkingAnimationFrames, this);
			SetIdle();
			SinkingDelta = -SinkingAnimation.GetFrameTexture(0).GetSize() / 2 + SinkingCorrection;
		}

		/// <summary>
		/// Start puffle lighting up animation
		/// </summary>
		public void Ignite()
		{
			IsIgniting = true;
			IgniteAnimation.Start();
		}

		public void Activate()
		{
			IsActive = true;
		}

		/// <summary>
		/// Start sinking into the tile (Death animation)
		/// </summary>
		public void Sink()
		{
			Position += SinkingDelta;
			IsSinking = true;
			SinkingAnimation.Start();
		}

		private void EndSink()
		{
			Position -= SinkingDelta;
			IsSinking = false;
		}

		/// <summary>
		/// Set puffle to idle animation
		/// </summary>
		private void SetIdle()
		{
			IsIdle = true;
			IdleAnimation.Start();
		}

		public override void _Process(double delta)
		{
			if (!IsActive)
			{
				return;
			}
			if (IsSinking)
			{
				var ended = SinkingAnimation.Advance();
				if (ended)
				{
					EndSink();
					Engine.ResetLevel();
				}
				return;
			}
			if (IsIgniting)
			{
				var ended = IgniteAnimation.Advance();
				if (ended)
				{
					IsIgniting = false;
					SetIdle();
				}
			}
			else if (IsIdle)
			{
				IdleAnimation.Advance();
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
		public bool CanMove(Direction direction)
		{
			var targetCoords = GetDestination(Coordinates, direction);
			var targetTile = Engine.GetTile(targetCoords);

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

		private void StartMoveAnimation(Direction direction)
		{
			var targetCoords = GetDestination(Coordinates, direction);
			var targetTile = Engine.GetTile(targetCoords);
			Engine.GetTile(Coordinates).OnPuffleExit();
			targetTile.OnPuffleStartEnter(direction);
			base.StartMoveAnimation(targetCoords, direction, targetTile.Position);
		}

		protected override void FinishMoveAnimation()
		{
			base.FinishMoveAnimation();
			Engine.GetTile(Coordinates).OnPuffleFinishEnter(this);
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

		public void TeleportTo(Vector2I coords)
		{
			Position = Engine.GetTile(coords).Position;
			Coordinates = coords;
		}

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

		public void Die()
		{
			IsMoving = false;
			ResetKeys();
		}

		public bool HasKeys()
		{
			return KeyCount > 0;
		}
	}
}
