using Godot;
using System;

public partial class ThinIcePlayButton : ThinIceButton
{


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnPressed()
	{
		ThinIceGame game = (ThinIceGame)GetNode("../../../ThinIceGame");
		game.StartLevel(1);
		game.Visible = true;
		Node menuNode = GetNode("../../");
		menuNode.QueueFree();
	}
}


