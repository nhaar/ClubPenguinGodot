using Godot;
using System;

/// <summary>
/// Object for a single tile in the Thin Ice game
/// </summary>
public partial class ThinIceTile : Sprite2D
{
	public ThinIceGame.TileType TileType { get; set; }

	/// <summary>
	/// Reference to a key object if this tile has one
	/// </summary>
	public Sprite2D KeyReference { get; set; }

	/// <summary>
	/// Reference to a block object if it's on this tile
	/// </summary>
	public ThinIceBlock BlockReference { get; set; }

	/// <summary>
	/// Reference to the teleporter this tile is linked to, if it is a teleporter
	/// </summary>
	public ThinIceTile LinkedTeleporter { get; set; }

	/// <summary>
	/// Whether or not this tile is a plaid teleporter
	/// </summary>
	public bool IsPlaidTeleporter { get; set; }

	/// <summary>
	/// Coordinate of the tile in the grid
	/// </summary>
	public Vector2I TileCoordinate { get; set; }

	/// <summary>
	/// Reference to the game
	/// </summary>
	public ThinIceGame Game { get; set; }

	/// <summary>
	/// Change the tile to a fresh new one of a given type
	/// </summary>
	/// <param name="tileType"></param>
	/// <exception cref="NotImplementedException"></exception>
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

	/// <summary>
	/// Action to perform when the puffle enters this tile
	/// </summary>
	/// <param name="direction"></param>
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
		
		// move if block exists
		BlockReference?.Move(direction);
		
		if (KeyReference != null)
		{
			RemoveKey();
			Game.Puffle.GetKey();
		}
	}

	/// <summary>
	/// Action to perform when the puffle exits this tile
	/// </summary>
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

	/// <summary>
	/// Make this teleporter a plaid teleporter
	/// </summary>
	public void MakePlaidTeleporter()
	{
		IsPlaidTeleporter = true;
		Texture = Game.PlaidTeleporterTile;
	}

	/// <summary>
	/// Remove key from tile
	/// </summary>
	public void RemoveKey()
	{
		if (KeyReference != null)
		{
			KeyReference.QueueFree();
			KeyReference = null;
		}
	}

	/// <summary>
	/// Add key to tile
	/// </summary>
	public void AddKey()
	{
		KeyReference = new Sprite2D();
		KeyReference.Texture = Game.KeyTexture;
		AddChild(KeyReference);
	}
}
