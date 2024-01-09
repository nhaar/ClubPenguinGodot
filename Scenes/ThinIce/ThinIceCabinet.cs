using Godot;
using System;

/// <summary>
/// Node for the Thin Ice Arcade Cabinet image.
/// </summary>
public partial class ThinIceCabinet : Sprite2D
{
	public override void _Ready()
	{
		// as per the original game, the cabinet is placed on top of a placeholder
		// image and moved so that it's top right edge is shared with the placeholder's
		// likewise, the parent is the placeholder image and this operation is to move it
		ThinIcePlaceholder parent = (ThinIcePlaceholder)GetParent();

		// divided by 2 to account for the fact that the position is the center of the image
		Vector2 delta = (Texture.GetSize() - parent.Texture.GetSize()) / 2;

		// if place on each other's center, the cabinet needs to be moved
		// to the left and down
		delta = new Vector2(-Math.Abs(delta.X), Math.Abs(delta.Y));
		Translate(delta);
	}
}
