using Godot;
using System;
using ClubPenguinPlus.Utils;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Scene for the in-game keys
	/// </summary>
	public partial class Key : Sprite2D
	{
		[Export]
		private SpriteFrames KeyAnimationFrames { get; set; }

		private FramerateBoundAnimation KeyAnimation { get; set; }

		public override void _Ready()
		{
			base._Ready();
			KeyAnimation = new(KeyAnimationFrames, this);
			KeyAnimation.Start();
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			KeyAnimation.Advance();
		}
	}
}
