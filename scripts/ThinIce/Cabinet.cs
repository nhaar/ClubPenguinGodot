using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Node for the Thin Ice Arcade Cabinet image.
	/// </summary>
	public partial class Cabinet : Node2D
	{
		public override void _Ready()
		{
			var cabinet = GetNode<Sprite2D>("Cabinet");
			// the original doesn't do this, it uses a placeholder, here we are properly centering
			// the game
			// scale image to fit the height of the screen
			Scale = new Vector2(1, 1) * 1080f / cabinet.Texture.GetSize().Y;

			// move the image to the center of the screen
			Translate(new Vector2(1920f / 2, 1080f / 2));
		}
	}
}
