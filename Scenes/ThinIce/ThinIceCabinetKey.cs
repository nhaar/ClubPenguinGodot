using Godot;
using System;

public partial class ThinIceCabinetKey : Sprite2D
{
	[Export]
	public Texture2D StillTexture { get; set; }
	
	[Export]
	public Texture2D PressedTexture { get; set; }
	
	[Export]
	public Key BoundKey { get; set; }
	
	public bool IsPressed { get; set; }

	public Vector2 PressDelta { get; set; }
	
	public override void _Ready()
	{
		Texture = StillTexture;
		IsPressed = false;

		PressDelta = StillTexture.GetSize() - PressedTexture.GetSize();

		GD.Print(PressDelta);
	}

	public override void _Process(double delta)
	{
		bool pressedNow = Input.IsPhysicalKeyPressed(BoundKey);
		if (pressedNow != IsPressed)
		{
			IsPressed = pressedNow;
			Texture = IsPressed ? PressedTexture : StillTexture;
			DisplaceButton(IsPressed);
		}
	}

	public void DisplaceButton(bool isOriginallyStill)
	{
		int sign = isOriginallyStill ? 1 : -1;
		Translate(PressDelta * sign / 2);
	}
}
