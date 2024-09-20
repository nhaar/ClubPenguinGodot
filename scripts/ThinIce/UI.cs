using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.ThinIce
{
	public partial class UI : Node2D
	{
		public Label PointsLabel { get; set; }

		public Label MeltedTileLabel { get; set; }

		public Label TotalTileLabel { get; set; }

		public Label LevelLabel { get; set; }

		public Game Engine { get; set; }

		public override void _Ready()
		{
			base._Ready();
			PointsLabel = GetNode<Label>("PointsLabel/PointsNumber");
			MeltedTileLabel = GetNode<Label>("ArcadeTopRow/TileSeparator/MeltedTileCount");
			TotalTileLabel = GetNode<Label>("ArcadeTopRow/TileSeparator/TotalTileCount");
			LevelLabel = GetNode<Label>("ArcadeTopRow/LevelLabel/LevelNumber");
			Engine = GetNode<Game>("%ThinIceEngine");
		}

		public override void _Process(double delta)
		{
			base._Process(delta);
			LevelLabel.Text = Engine.CurrentLevelNumber.ToString();
			MeltedTileLabel.Text = Engine.MeltedTiles.ToString();
			TotalTileLabel.Text = Engine.CurrentLevel.TotalTileCount.ToString();
			PointsLabel.Text = Engine.GetPoints().ToString();
		}
		private void OnResetButtonPressed()
		{
			Engine.Reset();
		}
	}
}


