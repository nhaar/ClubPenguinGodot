using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	public partial class HowTo : Node2D
	{
		[Export]
		public NodePath TitlePath { get; set; }

		[Export]
		public NodePath EnginePath { get; set; }

		[Export]
		public NodePath UIPath { get; set; }

		[Export]
		public NodePath LogoPath { get; set; }

		public Game Engine { get; set; }


		public override void _Ready()
		{
			Engine = GetNode<Game>(EnginePath);
		}

		public void StartHowTo()
		{
			var title = GetNode<Node2D>(TitlePath);
			title.QueueFree();
			Visible = true;
		}

		public void EndHowTo()
		{
			Engine.Visible = true;
			Engine.StartLevel(1);
			var ui = GetNode<Node2D>(UIPath);
			ui.Visible = true;
			var logo = GetNode<Node2D>(LogoPath);
			logo.QueueFree();
			QueueFree();
		}
	}
}
