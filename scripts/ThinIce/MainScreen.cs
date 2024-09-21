using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Scene for the main screen, which displays the tutorials, game and end screens
	/// </summary>
	public partial class MainScreen : Node2D
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

		private Engine Engine { get; set; }

		private UI UI { get; set; }

		private Node2D HowToScreen { get; set; }

		private Sprite2D Logo { get; set; }

		public override void _Ready()
		{
			Engine = GetNode<Engine>(EnginePath);
			Title = GetNode<Node2D>(TitlePath);
			UI = GetNode<UI>(UIPath);
			Logo = GetNode<Sprite2D>(LogoPath);
			HowToScreen = GetNode<Node2D>(HowToPath);
			UI.Engine = Engine;
		}

		/// <summary>
		/// Signal for pressing the START button
		/// </summary>
		private void StartHowTo()
		{
			Title.QueueFree();
			HowToScreen.Visible = true;
		}

		/// <summary>
		/// Signal for pressing the PLAY button
		/// </summary>
		private void EndHowTo()
		{
			Engine.Activate();
			UI.Visible = true;
			Logo.QueueFree();
			HowToScreen.QueueFree();
		}
	}
}
