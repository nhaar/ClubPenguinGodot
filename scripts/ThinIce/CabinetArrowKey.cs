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
		/// Key in the keyboard that is bound to this key
		/// </summary>
		[Export]
		private Godot.Key BoundKey { get; set; }

		/// <summary>
		/// Whether the key is currently physically pressed in-game
		/// </summary>
		private bool IsPressed { get; set; }

		/// <summary>
		/// A constant value that represents the difference in size between the still and 
		/// the pressed texture
		/// </summary>
		private Vector2 PressDelta { get; set; }

		public override void _Ready()
		{
			Texture = StillTexture;
			IsPressed = false;
			PressDelta = StillTexture.GetSize() - PressedTexture.GetSize();
		}

		public override void _Process(double delta)
		{
			bool pressedNow = Input.IsPhysicalKeyPressed(BoundKey);
			if (pressedNow != IsPressed)
			{
				IsPressed = pressedNow;
				Texture = IsPressed ? PressedTexture : StillTexture;
				DisplaceButton();
			}
		}

		/// <summary>
		/// Displaces the button to account for the size difference between the
		/// still and the pressed textures
		/// </summary>
		private void DisplaceButton()
		{
			int sign = IsPressed ? 1 : -1;
			// divide by 2 because the displacement is center-aligned
			Translate(PressDelta * sign / 2);
		}
	}
}
