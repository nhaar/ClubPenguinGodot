using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
    /// <summary>
    /// Object that displays the current number of melted tiles in the Thin Ice HUD
    /// </summary>
    public partial class MeltedTileCount : Text
    {
        [Export]
        public NodePath GamePath { get; set; }

        /// <summary>
        /// Reference to the game
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Tracker of the melted tile count value for display
        /// </summary>
        private int _currentMeltedTileCount;

        public override void _Ready()
        {
            Game = GetNode<Game>(GamePath);
            _currentMeltedTileCount = Game.MeltedTiles;
            Text = _currentMeltedTileCount.ToString();
            base._Ready();
        }

        public override void _Process(double delta)
        {
            if (_currentMeltedTileCount != Game.MeltedTiles)
            {
                _currentMeltedTileCount = Game.MeltedTiles;
                Text = Game.MeltedTiles.ToString();
            }
        }
    }
}
