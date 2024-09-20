using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	public partial class HowTo : Node2D
	{
		public Node2D Title { get; set; }

		public Game Engine { get; set; }

		public UI UI { get; set; }

		public Node2D HowToScreen { get; set; }

		public Sprite2D Logo { get; set; }

		public override void _Ready()
		{
			Engine = GetNode<Game>("ThinIceEngine");
			Title = GetNode<Node2D>("ThinIceTitle");
			UI = GetNode<UI>("UI");
			Logo = GetNode<Sprite2D>("ThinIceLogo");
			HowToScreen = GetNode<Node2D>("ThinIceHowTo");
		}

		public void StartHowTo()
		{
			Title.QueueFree();
			HowToScreen.Visible = true;
		}

		public void EndHowTo()
		{
			Engine.Visible = true;
			Engine.StartLevel(1);
			UI.Visible = true;
			Logo.QueueFree();
			HowToScreen.QueueFree();
		}
	}
}
