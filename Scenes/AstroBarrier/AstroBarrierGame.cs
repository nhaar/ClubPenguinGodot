using Godot;
using System;
using System.Collections.Generic;

public partial class AstroBarrierGame : Node
{
	[Export]
	public Texture2D TextureBullet {get; set;}

	[Export]
	public Texture2D TextureTarget {get; set;}

	[Export]
	public Texture2D TextureTargetDMG {get; set;}

	private AstroBarrierBullet _bullet = null;

	private List<AstroBarrierTarget> _targets = new();

	public AstroBarrierBullet Bullet {get{return _bullet;}}



	public void AddBullet(Vector2 shippos)
	{
		if(Bullet == null)
		{
			_bullet = new()
			{
				Position = shippos
			};

			AddChild(_bullet);
		}
		
	}

	/// <summary>
	/// pos1 and pos2 are the positions of the two colliding objects from the centre
	/// size1 and size2 are the size of the two colliding objects
	/// </summary>
	/// <param name="pos1"></param>
	/// <param name="size1"></param>
	/// <param name="pos2"></param>
	/// <param name="size2"></param>
	/// <returns></returns>
	private static bool IsColliding(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
	{
		Vector2 topleft1 = pos1 - size1 / 2;
		Vector2 topleft2 = pos2 - size2 / 2;

		return !(topleft1.X + size1.X <= topleft2.X ||
			topleft1.X >= topleft2.X + size2.X ||
			topleft1.Y + size1.Y <= topleft2.Y ||
			topleft1.Y >= topleft2.Y + size2.Y);
		
	}

	private bool HasBulletHitTarget(AstroBarrierTarget target)
	{
		if(_bullet == null)
			return false;
		else
		{
			Vector2 targetsize = TextureTarget.GetSize();
			Vector2 bulletsize = TextureBullet.GetSize();

			return IsColliding(target.Position, targetsize, Bullet.Position, bulletsize);
		}
	}

	private void RemoveBullet()
	{
		_bullet.QueueFree();
		_bullet = null;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_targets.Add(new AstroBarrierMidTarget());
		AddChild(_targets[^1]);
		_targets[^1].Position = new Vector2(0, -50);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(_bullet != null)
		{
			foreach(AstroBarrierTarget target in _targets)
			{
				if(HasBulletHitTarget(target))
				{
					RemoveBullet();
					target.Hit();
					return;
				}
			}


			if(_bullet.Position.Y < -230)
				RemoveBullet();
			
		}
		
	}
}
