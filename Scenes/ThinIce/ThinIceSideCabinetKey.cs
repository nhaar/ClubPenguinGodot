using Godot;
using System;

public partial class ThinIceSideCabinetKey : ThinIceCabinetKey
{
	public static readonly float VerticalPosition = 2200f;

	public static readonly float SidePosition = 2000f;

	[Export]
	public bool IsLeft { get; set; }

	public float XPosition => SidePosition * (IsLeft ? -1 : 1);

	public override void _Ready()
	{
		if (IsLeft)
		{
			Scale = new Vector2(-1, 1);
		}
		Position = new Vector2(XPosition, VerticalPosition);

		base._Ready();
	}
}
