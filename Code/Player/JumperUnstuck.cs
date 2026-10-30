using Sandbox;
using System;

public sealed partial class PlayerInput
{
	/// <summary>
	/// Button that triggers an unstuck attempt
	/// </summary>
	[Property, Feature( "Unstuck" ), InputAction] public string UnstuckButton { get; set; } = "unstuck";

	/// <summary>
	/// Furthest distance we'll search for a safe spot. Kept small so it can't be used to skip parts of the climb.
	/// </summary>
	[Property, Feature( "Unstuck" )] public float UnstuckSearchRadius { get; set; } = 256f;

	/// <summary>
	/// Minimum distance a candidate must be from the current position. Forces a real move out of the problem area.
	/// </summary>
	[Property, Feature( "Unstuck" )] public float UnstuckMinDistance { get; set; } = 24f;

	/// <summary>
	/// Extra clearance added around the body so we don't land touching geometry and get stuck again.
	/// </summary>
	[Property, Feature( "Unstuck" )] public float UnstuckPadding { get; set; } = 4f;

	/// <summary>
	/// Distance between sampled positions
	/// </summary>
	[Property, Feature( "Unstuck" )] public float UnstuckStep { get; set; } = 8f;

	[Property, Feature( "Unstuck" )] public float UnstuckCooldown { get; set; } = 0.5f;

	TimeSince timeSinceUnstuck = 10;

	void CheckUnstuckInput()
	{
		if ( Input.Pressed( UnstuckButton ) )
			Unstuck();
	}

	/// <summary>
	/// Moves the player to the nearest position where their body fits and that has ground underneath.
	/// Returns true if the player was moved.
	/// </summary>
	public bool Unstuck()
	{
		if ( IsProxy ) return false;
		if ( timeSinceUnstuck < UnstuckCooldown )
		{
			Log.Info( "Unstuck: on cooldown" );
			return false;
		}
		timeSinceUnstuck = 0;

		var origin = WorldPosition;
		var bestDistSq = float.MaxValue;
		Vector3? best = null;

		// Sample a grid around the player and keep the closest valid spot.
		// Brute force is fine here, it only runs on demand.
		var steps = (int)MathF.Ceiling( UnstuckSearchRadius / UnstuckStep );
		for ( int x = -steps; x <= steps; x++ )
			for ( int y = -steps; y <= steps; y++ )
				for ( int z = -steps; z <= steps; z++ )
				{
					var offset = new Vector3( x, y, z ) * UnstuckStep;
					var distSq = offset.LengthSquared;
					if ( distSq >= bestDistSq || distSq > UnstuckSearchRadius * UnstuckSearchRadius )
						continue;

					var candidate = origin + offset;
					if ( !TryGetSafeSpot( candidate, out var spot ) )
						continue;

					// Ignore our current spot, the player pressed the button because something's wrong here
					if ( spot.Distance( origin ) < UnstuckMinDistance )
						continue;

					bestDistSq = distSq;
					best = spot;
				}

		// Nothing nearby, fall back to the last place we were standing
		if ( best is null && TryGetSafeSpot( LastGroundedPos + Vector3.Up * UnstuckStep, out var fallback ) )
			best = fallback;

		if ( best is null )
		{
			Log.Warning( $"Unstuck: no safe spot found within {UnstuckSearchRadius} of {origin}" );
			return false;
		}

		Log.Info( $"Unstuck: {origin} -> {best.Value}" );

		WorldPosition = best.Value;
		Transform.ClearInterpolation();
		Controller.Body.Velocity = 0;
		Controller.WishVelocity = 0;
		return true;
	}

	// Padded body hull, raised by the padding so the bottom doesn't graze the floor we're standing on
	BBox UnstuckHull => new BBox(
		new Vector3( -Controller.BodyRadius - UnstuckPadding, -Controller.BodyRadius - UnstuckPadding, UnstuckPadding ),
		new Vector3( Controller.BodyRadius + UnstuckPadding, Controller.BodyRadius + UnstuckPadding, Controller.BodyHeight + UnstuckPadding ) );

	bool TryGetSafeSpot( Vector3 position, out Vector3 spot )
	{
		spot = default;
		var hull = UnstuckHull;

		// Body must not overlap anything
		var fit = Scene.Trace.Box( hull, position, position )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "player" )
			.Run();

		if ( fit.StartedSolid || fit.Hit )
			return false;

		// Must have standable ground just below, so we don't drop the player into a fall
		var down = Scene.Trace.Box( hull, position, position + Vector3.Down * (UnstuckStep * 2f) )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "trigger", "player" )
			.Run();

		if ( !down.Hit || down.StartedSolid )
			return false;

		if ( Vector3.GetAngle( Vector3.Up, down.Normal ) > 45f )
			return false;

		spot = down.EndPosition + Vector3.Up * 0.1f;
		return true;
	}
}
