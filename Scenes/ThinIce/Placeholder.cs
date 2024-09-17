using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Class for the placeholder image that is used to set the scale
	/// and position of the Thin Ice images
	/// </summary>
	public partial class Placeholder : Sprite2D
	{
		public override void _Ready()
		{
			// scale image to fit the height of the screen
			Scale = new Vector2(1, 1) * 1080f / Texture.GetSize().Y;

			// move the image to the center of the screen
			Translate(new Vector2(1920f / 2, 1080f / 2));
		}
	}
}
