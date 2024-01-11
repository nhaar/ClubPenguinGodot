using Godot;
using System;

public partial class AstroBarrierMidTarget : AstroBarrierTarget
{

	public override void Hit()
	{
		Texture = GetParent<AstroBarrierGame>().TextureTargetDMG;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Texture = GetParent<AstroBarrierGame>().TextureTarget;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
