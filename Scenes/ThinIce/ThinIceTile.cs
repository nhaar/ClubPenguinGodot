using Godot;
using System;

public partial class ThinIceTile : Sprite2D
{
	public ThinIceGame.TileType TileType { get; set; }

	public Sprite2D KeyReference { get; set; }

	public ThinIceBlock BlockReference { get; set; }

	public ThinIceTile LinkedTeleporter { get; set; }

	public bool IsPlaidTeleporter { get; set; }

	public Vector2I TileCoordinate { get; set; }

	public ThinIceGame Game { get; set; }

	public void ChangeTile(ThinIceGame.TileType tileType)
	{
		RemoveKey();
		LinkedTeleporter = null;
		BlockReference = null;
		IsPlaidTeleporter = false;

		Texture2D tileTexture = tileType switch
		{
			ThinIceGame.TileType.Empty => Game.EmptyTile,
			ThinIceGame.TileType.Ice => Game.IceTile,
			ThinIceGame.TileType.Water => Game.WaterTile,
			ThinIceGame.TileType.ThickIce => Game.ThickIceTile,
			ThinIceGame.TileType.Wall => Game.WallTile,
			ThinIceGame.TileType.Goal => Game.GoalTile,
			ThinIceGame.TileType.Teleporter => Game.TeleporterTile,
			ThinIceGame.TileType.PlaidTeleporter => Game.PlaidTeleporterTile,
			ThinIceGame.TileType.Lock => Game.LockTile,
			ThinIceGame.TileType.Button => Game.ButtonTile,
			ThinIceGame.TileType.FakeWall => Game.WallTile,
			ThinIceGame.TileType.BlockHole => Game.BlockHoleTile,
			_ => throw new NotImplementedException(),
		};

		Texture = tileTexture;
		TileType = tileType;
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnPuffleEnter(ThinIcePuffle.Direction direction)
	{
		if (TileType == ThinIceGame.TileType.Goal)
		{
			Game.GoToNextLevel();
		}
		// we must have the key already
		// also, this functionality should be changed to work on adjacent tile enter
		else if (TileType == ThinIceGame.TileType.Lock)
		{
			ChangeTile(ThinIceGame.TileType.Ice);
			Game.Puffle.UseKey();
		}
		else if (!IsPlaidTeleporter && TileType == ThinIceGame.TileType.Teleporter)
		{
			Game.Puffle.TeleportTo(LinkedTeleporter.TileCoordinate);
			MakePlaidTeleporter();
			LinkedTeleporter.MakePlaidTeleporter();
		}
		if (BlockReference != null)
		{
			BlockReference.Move(direction);
		}
		if (KeyReference != null)
		{
			RemoveKey();
			Game.Puffle.GetKey();
		}
	}

	public void OnPuffleExit()
	{
		if (TileType == ThinIceGame.TileType.Ice)
		{
			ChangeTile(ThinIceGame.TileType.Water);
		}
		else if (TileType == ThinIceGame.TileType.ThickIce)
		{
			ChangeTile(ThinIceGame.TileType.Ice);
		}
	}

	public void MakePlaidTeleporter()
	{
		IsPlaidTeleporter = true;
		Texture = Game.PlaidTeleporterTile;
	}

	public void RemoveKey()
	{
		if (KeyReference != null)
		{
			KeyReference.QueueFree();
			KeyReference = null;
		}
	}

	public void AddKey()
	{
		KeyReference = new Sprite2D();
		KeyReference.Texture = Game.KeyTexture;
		AddChild(KeyReference);
	}
}
