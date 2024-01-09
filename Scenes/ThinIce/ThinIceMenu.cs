using Godot;
using System;

public partial class ThinIceMenu : Node2D
{
	public void OpenInstructionMenu()
	{
		GetNode("ThinIceMainMenu").QueueFree();
		((Node2D)GetNode("ThinIceInstructionMenu")).Visible = true;
	}
}
