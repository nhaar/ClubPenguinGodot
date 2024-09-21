using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
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

		private Label PointsLabel { get; set; }

		private Label MeltedTileLabel { get; set; }

		private Label TotalTileLabel { get; set; }

		private Label LevelLabel { get; set; }

		public Engine Engine { get; set; }

		public override void _Ready()
		{
			base._Ready();
			PointsLabel = GetNode<Label>(PointsPath);
			MeltedTileLabel = GetNode<Label>(MeltedTilePath);
			TotalTileLabel = GetNode<Label>(TotalTilePath);
			LevelLabel = GetNode<Label>(LevelPath);
		}

		public override void _Process(double delta)
		{
			LevelLabel.Text = Engine.CurrentLevelNumber.ToString();
			MeltedTileLabel.Text = Engine.MeltedTiles.ToString();
			TotalTileLabel.Text = Engine.TotalTileCount.ToString();
			PointsLabel.Text = Engine.Points.ToString();
		}
		private void OnResetButtonPressed()
		{
			Engine.ResetLevel();
		}
	}
}


