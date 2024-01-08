using Godot;
using System;

public partial class ThinIceImage : Sprite2D
{
	// hardcoded scale that uses all images were exported with 1000% zoom
	// and it uses that the biggest image is supposed to fill the screen vertically
	// and it has 4801 of height and we are fitting for a 1920x1080 screen
	public static readonly float ImageScale = 1080f / 4801f;
	
	[Export]
	// uses a scale from -1 to 1 in each axis
	// such that (0, 0) corresponds to the center
	public Vector2 ThinIcePosition { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Scale = new Vector2(1, 1) * ImageScale;
		// dividing by 2 because the scale is from -1 to 1
		Vector2 scaledPosition = ThinIcePosition / 2;
		Translate(new Vector2(1920f * (scaledPosition.X + 0.5f), 1080f * (scaledPosition.Y + 0.5f)));
	}

	public Vector2 GetThinIceSize()
	{
		Vector2 spriteSize = Texture.GetSize();
		return new Vector2(spriteSize.X * ImageScale, spriteSize.Y * ImageScale);
	}
}
