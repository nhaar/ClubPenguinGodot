using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Class for labels used in the game UI
	/// </summary>
	public partial class GameText : Label
	{
		[Export]
		private Font Font;

		private static readonly Color TextColor = new(0, 102f / 255, 204f / 255);

		public override void _Ready()
		{
			LabelSettings = new()
			{
				Font = Font,
				FontSize = 266,
				FontColor = TextColor
			};
		}
	}
}
