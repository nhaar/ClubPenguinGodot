using Godot;
using System;

/// <summary>
/// Object for the medium-sized targets (as opposed to wide and small)
/// </summary>
public partial class AstroBarrierMediumTarget : AstroBarrierTarget
{
	private AstroBarrierGame Game { get; set; }

	public override void Hit()
	{
		Texture = Game.DamagedTargetTexture;
	}

	public override void _Ready()
	{
		Game = GetParent<AstroBarrierGame>();
        Texture = Game.TargetTexture;
	}
}
