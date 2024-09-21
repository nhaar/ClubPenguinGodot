using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// Scene for the in-game keys
	/// </summary>
	public partial class Key : Sprite2D
	{
		[Export]
		public Texture2D KeyTexture { get; set; }

		public override void _Ready()
		{
			base._Ready();
			Texture = KeyTexture;
		}
	}
}
