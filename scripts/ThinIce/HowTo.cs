using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	public partial class HowTo : Node2D
	{
		[Export]
		private NodePath EnginePath;

		[Export]
		private NodePath TitlePath;

		[Export]
		private NodePath UIPath;

		[Export]
		private NodePath LogoPath;

		[Export]
		private NodePath HowToPath;

		private Node2D Title { get; set; }

		private Game Engine { get; set; }

		private UI UI { get; set; }

		private Node2D HowToScreen { get; set; }

		private Sprite2D Logo { get; set; }

		public override void _Ready()
		{
			Engine = GetNode<Game>(EnginePath);
			Title = GetNode<Node2D>(TitlePath);
			UI = GetNode<UI>(UIPath);
			Logo = GetNode<Sprite2D>(LogoPath);
			HowToScreen = GetNode<Node2D>(HowToPath);
			UI.Engine = Engine;
		}

		private void StartHowTo()
		{
			Title.QueueFree();
			HowToScreen.Visible = true;
		}

		private void EndHowTo()
		{
			Engine.MakeVisible();
			UI.Visible = true;
			Logo.QueueFree();
			HowToScreen.QueueFree();
		}
	}
}
