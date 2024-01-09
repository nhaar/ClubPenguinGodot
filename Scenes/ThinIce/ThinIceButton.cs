using Godot;
using System;

public partial class ThinIceButton : TextureButton
{
	[Export]
	public string ButtonText { get; set; }

	[Export]
	public Font ButtonFont { get; set; }

	[Export]
	public int ButtonFontSize { get; set; }

	public override void _Ready()
	{
		Bitmap bitmap = new Bitmap();
		bitmap.CreateFromImageAlpha(TextureNormal.GetImage());
		TextureClickMask = bitmap;
		
		Label label = new Label();
		label.Text = ButtonText;
		label.LabelSettings = new LabelSettings();
		label.LabelSettings.Font = ButtonFont;
		label.LabelSettings.FontSize = ButtonFontSize;
		label.VerticalAlignment = VerticalAlignment.Center;
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.SetSize(TextureNormal.GetSize());

		AddChild(label);

		base._Ready();
	}
}
