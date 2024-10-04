using Godot;
using System;

/// <summary>
/// Base class for all targets in Astro Barrier
/// </summary>
public abstract partial class AstroBarrierTarget : Sprite2D
{
	/// <summary>
	/// Abstract method for when the target is hit by a bullet
	/// </summary>
	public abstract void Hit();
}
