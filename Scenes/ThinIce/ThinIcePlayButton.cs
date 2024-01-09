using Godot;
using System;

public partial class ThinIcePlayButton : ThinIceButton
{
	private void OnPressed()
	{
		ThinIceGame game = (ThinIceGame)GetNode("../../../ThinIceGame");
		game.StartLevel(1);
		game.Visible = true;
		Node menuNode = GetNode("../../");
		menuNode.QueueFree();
	}
}


