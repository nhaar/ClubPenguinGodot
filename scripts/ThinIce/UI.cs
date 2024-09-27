using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Scene that handles the game information and UI
	/// </summary>
	public partial class UI : Node2D
	{
		[Export]
		private NodePath PointsPath { get; set; }

		[Export]
		private NodePath MeltedTilePath { get; set; }

		[Export]
		private NodePath TotalTilePath { get; set; }

		[Export]
		private NodePath LevelPath { get; set; }

		[Export]
		private NodePath SolvedPath { get; set; }

		private Label PointsLabel { get; set; }

		private Label MeltedTileLabel { get; set; }

		private Label TotalTileLabel { get; set; }

		private Label LevelLabel { get; set; }

		private Label SolvedLabel { get; set; }

		public Engine Engine { get; set; }

		public override void _Ready()
		{
			base._Ready();
			PointsLabel = GetNode<Label>(PointsPath);
			MeltedTileLabel = GetNode<Label>(MeltedTilePath);
			TotalTileLabel = GetNode<Label>(TotalTilePath);
			LevelLabel = GetNode<Label>(LevelPath);
			SolvedLabel = GetNode<Label>(SolvedPath);
		}

		public override void _Process(double delta)
		{
			LevelLabel.Text = Engine.CurrentLevelNumber.ToString();
			MeltedTileLabel.Text = Engine.MeltedTiles.ToString();
			TotalTileLabel.Text = Engine.TotalTileCount.ToString();
			PointsLabel.Text = Engine.DisplayPoints.ToString();
			SolvedLabel.Text = Engine.SolvedLevels.ToString();
		}

		/// <summary>
		/// Signal for pressing the button
		/// </summary>
		private void OnResetButtonPressed()
		{
			Engine.ResetLevel();
		}
	}
}


