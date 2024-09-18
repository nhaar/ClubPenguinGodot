using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
    /// <summary>
    /// Object that displays the current points number in the Thin Ice HUD
    /// </summary>
    public partial class PointsNumber : Text
    {
        /// <summary>
        /// Reference to the game
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Tracker of the points number value for display
        /// </summary>
        private int _currentPoints;

        public override void _Ready()
        {
            base._Ready();
            Game = GetParent<Label>().GetParent<Game>();
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
}
