using Godot;
using System;

/// <summary>
/// Object for the bullet shot by the Astro Barrier ship
/// </summary>
public partial class AstroBarrierBullet : Sprite2D
{
    /// <summary>
    /// Units of pixels per frame
    /// </summary>
    public Vector2 Velocity { get; private set; } = new(0, -20);

    public AstroBarrierGame Game { get; set; }

	public override void _Ready()
	{
        Game = GetParent<AstroBarrierGame>();
        Texture = Game.BulletTexture;
	}

    public bool HasBulletHitTarget(AstroBarrierTarget target)
    {
        return AstroBarrierGame.IsColliding(this, target);
    }

	public override void _Process(double delta)
	{
        Translate(Velocity);

        foreach (AstroBarrierTarget target in Game.Targets)
        {
            if (HasBulletHitTarget(target))
            {
                Despawn();
                target.Hit();
                return;
            }
        }

        // as per the original game, the bullet is removed when it goes offscreen
        if (Position.Y < -230)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        Game.Bullet = null;
        QueueFree();
    }
}
