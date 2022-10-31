
using SandboxEditor;
using Sandbox;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace JumpingSausage;

[Library( "js_ground", Description = "Ground Level" )]
[EditorSprite( "materials/editor/assault_rally.vmat" )]
[Display( Name = "Ground Level", GroupName = "Jumper", Description = "Ground Level" ), Category( "Gameplay" ), Icon( "grass" )]
[HammerEntity]
[BoundsHelper( "mins", "maxs", false, true )]
partial class MusicBoxTweaker : ModelEntity
{

	[Property( "mins", Title = "Mins" )]
	[Net]
	[DefaultValue( "-64 -64 -64" )]
	public Vector3 Mins { get; set; } = new Vector3( -64, -64, -64 );

	[Property( "maxs", Title = "Maxs" )]
	[Net]
	[DefaultValue( "64 64 64" )]
	public Vector3 Maxs { get; set; } = new Vector3( 64, 64, 64 );

	[Net]
	public BBox Outer { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Outer = new BBox( Position + Mins, Position + Maxs );

	}

	[Event.Frame]
	public void OnFrame()
	{
		var pos = CurrentView.Position;
		if ( Local.Pawn.IsValid() )
		{
			pos = Local.Pawn.Position;
		}

		var bbox = Outer;

		var dist = ShortestDistanceToSurface( bbox, pos );
		var vol = dist.Clamp( 0, 1 );

		var btw = Vector3.DistanceBetween( dist, pos );

		var pawn = Local.Pawn as JumpingSausagePawn;

		pawn.Height = btw.CeilToInt();

		if ( BasePlayerController.Debug )
		{

			DebugOverlay.Box( bbox, Color.Green );
			DebugOverlay.Text( vol.ToString(), bbox.Center, 0, 3000 );
		}
	}

	private Vector3 ShortestDistanceToSurface( BBox bbox, Vector3 position )
	{
		var result = Vector3.Zero;

		var outerclosetsPoint1 = Outer.ClosestPoint( position );

		result = outerclosetsPoint1;

		if ( BasePlayerController.Debug )
		{
			DebugOverlay.Text( result.ToString(), bbox.Center, 0, 3000 );
			DebugOverlay.Sphere( outerclosetsPoint1, 3f, Color.Blue, 0, false );
			DebugOverlay.Sphere( position, 3f, Color.Cyan, 0, false );


			DebugOverlay.Line( outerclosetsPoint1, position, 0f, false );


			DebugOverlay.Box( Outer, Color.Green );
		}

		return result;
	}

}
