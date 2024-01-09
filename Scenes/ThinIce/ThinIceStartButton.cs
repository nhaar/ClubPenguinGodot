using Godot;
using System;

public partial class ThinIceStartButton : ThinIceButton
{
	private void OnPressed()
	{
		Node2D mainMenu = (Node2D)GetParent();
		Node2D instructionMenu = (Node2D)mainMenu.GetNode("../ThinIceInstructionMenu");
		instructionMenu.Visible = true;
		mainMenu.QueueFree();
	}
}
