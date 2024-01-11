using Godot;
using System;

public partial class AstroBarrierBullet : Sprite2D
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Texture = GetParent<AstroBarrierGame>().TextureBullet;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Translate(new Vector2(0,-20));
	}
}
