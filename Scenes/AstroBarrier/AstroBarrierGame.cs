using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Object that controls the Astro Barrier game
/// </summary>
public partial class AstroBarrierGame : Node
{
	[Export]
	public Texture2D BulletTexture { get; set; }

	[Export]
	public Texture2D TargetTexture { get; set; }

	[Export]
	public Texture2D DamagedTargetTexture { get; set; }

	public List<AstroBarrierTarget> Targets { get; } = new();

	public AstroBarrierBullet Bullet { get; set; } = null;

	public AstroBarrierShip Ship { get; set; } = null;

	public void AddBullet()
	{
		// this requires for the bullet to be removed before another one can be added
		// (as in the original game)
		if (Bullet == null)
		{
			Bullet = new()
			{
				Position = Ship.Position
			};

			AddChild(Bullet);
		}
		
	}

	/// <summary>
	/// Checks for the collision of two objects with square hitboxes. Given positions are centered.
	/// </summary>
	/// <param name="obj1"></param>
	/// <param name="obj2"></param>
	/// <returns></returns>
	public static bool IsColliding(Sprite2D obj1, Sprite2D obj2)
	{
		Sprite2D[] objs = new[] { obj1, obj2 };
		Vector2[] sizes = objs.Select(obj => obj.Texture.GetSize()).ToArray();
		Vector2[] toplefts = objs.Select((obj, i) => obj.Position - sizes[i] / 2).ToArray();

		return !(toplefts[0].X + sizes[0].X <= toplefts[1].X ||
			toplefts[0].X >= toplefts[1].X + sizes[1].X ||
			toplefts[0].Y + sizes[0].Y <= toplefts[1].Y ||
			toplefts[0].Y >= toplefts[1].Y + sizes[1].Y);
	}

	public override void _Ready()
	{
		Ship = GetNode<AstroBarrierShip>("Ship");

		// temp target layout
		AstroBarrierMediumTarget target = new();
		Targets.Add(target);
		AddChild(target);
		target.Position = new Vector2(0, -50);
	}

	public override void _Process(double delta)
	{
	}
}
