using Godot;
using System;

namespace ClubPenguinPlus.ThinIce
{
	/// <summary>
	/// A button that is clickable in the Thin Ice game.
	/// </summary>
	public partial class Button : TextureButton
	{
		[Export]
		public string ButtonText { get; set; }

		[Export]
		public Font ButtonFont { get; set; }

		[Export]
		public int ButtonFontSize { get; set; }

		public override void _Ready()
		{
			// to delineate what is the button, use the alpha channel of the normal texture
			Bitmap bitmap = new();
			bitmap.CreateFromImageAlpha(TextureNormal.GetImage());
			TextureClickMask = bitmap;

			Label label = new()
			{
				Text = ButtonText,
				LabelSettings = new()
				{
					Font = ButtonFont,
					FontSize = ButtonFontSize
				},
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			label.SetSize(TextureNormal.GetSize());
			AddChild(label);

			base._Ready();
		}
	}
}
