using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// One of the arrow keys in the cabinet
	/// </summary>
	public partial class CabinetArrowKey : Sprite2D
	{
		/// <summary>
		/// Texture when the key is not pressed
		/// </summary>
		[Export]
		private Texture2D StillTexture { get; set; }

		/// <summary>
		/// Texture when the key is pressed
		/// </summary>
		[Export]
		private Texture2D PressedTexture { get; set; }

		/// <summary>
		/// When the key gets released
		/// </summary>
		[Export]
		private Texture2D ReleaseTexture { get; set; }

		/// <summary>
		/// Key in the keyboard that is bound to this key
		/// </summary>
		[Export]
		private Godot.Key BoundKey { get; set; }

		/// <summary>
		/// Whether the key is currently physically pressed in-game
		/// </summary>
		private bool IsPressed { get; set; }

		private bool IsReleasing { get; set; } = false;

		public override void _Ready()
		{
			Texture = StillTexture;
			IsPressed = false;
		}

		public override void _Process(double delta)
		{
			bool pressedNow = Input.IsPhysicalKeyPressed(BoundKey);
			if (pressedNow != IsPressed)
			{
				IsPressed = pressedNow;
				if (pressedNow)
				{
					IsReleasing = false;
					Texture = PressedTexture;
				}
				else
				{
					IsReleasing = true;
					Texture = ReleaseTexture;
				}
			}
			else if (IsReleasing)
			{
				IsReleasing = false;
				Texture = StillTexture;
			}
		}
	}
}
