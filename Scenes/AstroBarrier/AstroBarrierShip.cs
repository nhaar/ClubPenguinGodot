using Godot;
using System;

public partial class AstroBarrierShip : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsPhysicalKeyPressed(Key.Left))
		{
			Translate(new Vector2(-10, 0));
		}
		else if(Input.IsPhysicalKeyPressed(Key.Right))
		{
			Translate(new Vector2(10, 0));
		}
	}
}
