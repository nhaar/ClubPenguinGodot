using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Node for the Thin Ice Arcade Cabinet image.
	/// </summary>
	public partial class Cabinet : Node2D
	{
		[Export]
		public NodePath CabinetPath { get; set; }

		public override void _Ready()
		{
			var cabinet = GetNode<Sprite2D>(CabinetPath);
			// the original doesn't do this, it uses a placeholder, here we are properly centering
			// the game
			// scale image to fit the height of the screen
			var screen = DisplayServer.ScreenGetSize();
			
			Scale = new Vector2(1, 1) * screen.Y / cabinet.Texture.GetSize().Y;

			// move the image to the center of the screen
			Translate(screen / 2);
		}
	}
}
