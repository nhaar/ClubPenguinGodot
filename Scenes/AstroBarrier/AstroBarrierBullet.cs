using Godot;
using System;

public partial class AstroBarrierBullet : Sprite2D
{

	private Vector2 _velocity = new(0, -20);

	public Vector2 Velocity { get {return _velocity;} }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Texture = GetParent<AstroBarrierGame>().TextureBullet;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Translate(Velocity);
	}
}
