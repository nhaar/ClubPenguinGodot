using Godot;
using System;

public partial class AstroBarrierGame : Node
{
	[Export]
	public Texture2D TextureBullet {get; set;}

	private AstroBarrierBullet _bullet = null;

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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(_bullet != null)
		{
			if(_bullet.Position.Y < -230)
			{
				_bullet.QueueFree();
				_bullet = null;
			}
		}
		
	}
}
