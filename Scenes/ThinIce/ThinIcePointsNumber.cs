using Godot;
using System;

/// <summary>
/// Object that displays the current points number in the Thin Ice HUD
/// </summary>
public partial class ThinIcePointsNumber : ThinIceLabel
{
	/// <summary>
	/// Reference to the game
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Tracker of the points number value for display
	/// </summary>
	private int _currentPoints;

	public override void _Ready()
	{
		base._Ready();
		Game = GetParent<Label>().GetParent<ThinIceGame>();
		_currentPoints = Game.GetPoints();
	}

	public override void _Process(double delta)
	{
		int newPoints = Game.GetPoints();
		if (_currentPoints != newPoints)
		{
			_currentPoints = newPoints;
			Text = newPoints.ToString();
		}
	}
}
