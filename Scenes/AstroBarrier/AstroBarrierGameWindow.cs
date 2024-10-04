using Godot;
using System;

public partial class AstroBarrierGameWindow : Sprite2D
{
	public override void _Ready()
	{
		// values from FFDEC
		Scale = Vector2.One * 45875 / 65536;
	}

}
