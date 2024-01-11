using Godot;
using System;

public abstract partial class AstroBarrierTarget : Sprite2D
{


	public abstract void Hit();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
