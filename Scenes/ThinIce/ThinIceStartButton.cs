using Godot;
using System;

public partial class ThinIceStartButton : ThinIceButton
{
	private void _OnPressed()
	{
		GetTree().CallGroup("thin_ice_menu", "OpenInstructionMenu");
	}
}
