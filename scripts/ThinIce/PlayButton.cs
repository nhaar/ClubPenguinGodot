using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	public partial class PlayButton : Button
	{
		private void OnPressed()
		{
			Game game = (Game)GetNode("../../../ThinIceGame");
			game.StartLevel(1);
			game.Visible = true;
			Node menuNode = GetNode("../../");
			menuNode.QueueFree();
		}
	}
}
