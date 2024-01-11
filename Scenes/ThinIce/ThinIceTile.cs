using Godot;
using System;
using System.Linq;

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
	/// Reference to the coin bag on this tile, or null if none exists
	/// </summary>
	public Sprite2D CoinBag { get; set; } = null;

	/// <summary>
	/// Change the tile to a fresh new one of a given type
	/// </summary>
	/// <param name="tileType"></param>
	/// <exception cref="NotImplementedException"></exception>
	public void ChangeTile(ThinIceGame.TileType tileType)
	{
		RemoveKey();
		RemoveCoinBag();
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
			ThinIceGame.TileType.FakeTemporaryWall => Game.WallTile,
			ThinIceGame.TileType.FakeImpassableWall => Game.WallTile,
			ThinIceGame.TileType.FakePassableWall => Game.WallTile,
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
		if (CoinBag != null)
		{
			GetCoinBag();
		}
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
		else if (Game.Puffle.IsStuck())
		{
			Game.ResetLevel();
		}

		if (KeyReference != null)
		{
			RemoveKey();
			Game.Puffle.GetKey();
		}
	}

	/// <summary>
	/// Action to perform when the puffle exits this tile
	/// </summary>
	public void OnPuffleExit(ThinIcePuffle.Direction direction)
	{
		if (TileType == ThinIceGame.TileType.Ice)
		{
			Game.MeltTile();
			ChangeTile(ThinIceGame.TileType.Water);
		}
		else if (TileType == ThinIceGame.TileType.ThickIce)
		{
			Game.MeltTile();
			ChangeTile(ThinIceGame.TileType.Ice);
		}

		ThinIceTile destinationTile = GetAdjacent(direction);
		
		// move if block exists
		destinationTile.BlockReference?.Move(direction);
	
		// button is pressed on previous tile exit
		if (destinationTile.TileType == ThinIceGame.TileType.Button)
		{
			Game.PressButton();
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

	/// <summary>
	/// Get adjacent tile in a given direction
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	public ThinIceTile GetAdjacent(ThinIcePuffle.Direction direction)
	{
		return Game.GetTile(ThinIcePuffle.GetDestination(TileCoordinate, direction));
	}

	public void RemoveCoinBag()
	{
		if (CoinBag != null)
		{
			CoinBag.QueueFree();
			CoinBag = null;
		}
	}

	/// <summary>
	/// Removes and returns the value of the coin bag
	/// </summary>
	public void GetCoinBag()
	{
		Game.PointsInLevel += 100;
		RemoveCoinBag();
	}
}
