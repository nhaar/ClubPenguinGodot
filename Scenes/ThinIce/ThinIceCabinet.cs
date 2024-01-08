using Godot;
using System;

public partial class ThinIceCabinet : Sprite2D
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ThinIceImage parent = (ThinIceImage)GetParent();


		// divided by 2 to account for the fact that the position is the center of the image
		Vector2 delta = (Texture.GetSize() - parent.Texture.GetSize()) / 2;
		delta = new Vector2(-Math.Abs(delta.X), Math.Abs(delta.Y));
		Translate(delta);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
