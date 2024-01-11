using Godot;
using System;

/// <summary>
/// Object that displays the total tile count in the Thin Ice HUD
/// </summary>
public partial class ThinIceTotalTileCount : ThinIceLabel
{
	/// <summary>
	/// Reference to the game
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Tracker of the current level number for reference
	/// </summary>
	private int _currentLevel;

	public override void _Ready()
	{
		Game = GetNode<ThinIceGame>("../../../");
		_currentLevel = Game.CurrentLevelNumber;
		Text = Game.CurrentLevel.TotalTileCount.ToString();
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (_currentLevel != Game.CurrentLevelNumber)
		{
			_currentLevel = Game.CurrentLevelNumber;
			Text = Game.CurrentLevel.TotalTileCount.ToString();
		}
	}
}
