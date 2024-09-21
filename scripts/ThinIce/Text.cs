using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Class for labels used in the Thin Ice game buttons
	/// </summary>
	public partial class Text : Label
	{
		[Export]
		private Font Font;

		private static readonly Color TextColor = new(0, 102f / 255, 204f / 255);

		public override void _Ready()
		{
			LabelSettings = new()
			{
				Font = Font,
				FontSize = 160,
				FontColor = TextColor
			};
		}
	}
}
