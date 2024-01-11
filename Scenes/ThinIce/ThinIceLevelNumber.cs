using Godot;
using System;

/// <summary>
/// Object that displays the current level number in the Thin Ice HUD
/// </summary>
public partial class ThinIceLevelNumber : ThinIceLabel
{
	/// <summary>
	/// Reference to the game
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Tracker of the level number value for display
	/// </summary>
	private int _currentLevel;

	public override void _Ready()
	{
		base._Ready();
		Game = GetParent<ThinIceGame>();
		_currentLevel = Game.CurrentLevelNumber;
	}

	public override void _Process(double delta)
	{
		if (_currentLevel != Game.CurrentLevelNumber)
		{
			_currentLevel = Game.CurrentLevelNumber;
			Text = Game.CurrentLevelNumber.ToString();
		}
	}
}
