using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Class for one of side cabinet keys.
	/// </summary>
	public partial class SideCabinetArrowKey : CabinetArrowKey
	{
		/// <summary>
		/// Whether this is the left key or the right key
		/// </summary>
		[Export]
		private bool IsLeft { get; set; }

		public override void _Ready()
		{
			if (IsLeft)
			{
				Scale = new Vector2(-1, 1);
			}

			base._Ready();
		}
	}
}
