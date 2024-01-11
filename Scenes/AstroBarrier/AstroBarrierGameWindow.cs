using Godot;
using System;

public partial class AstroBarrierGameWindow : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Scale = Vector2.One * 45875 / 65536;
		GD.Print(Position);
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
