using Sandbox;
using System;

[Title( "Circle Forward Emitter" )]
[Category( "Particles" )]
public sealed class ParticleCircleForwardEmitter : ParticleEmitter
{
	[Property, Group( "Circle" )]
	public float Radius { get; set; } = 10.0f;


	protected override void DrawGizmos()
	{
		if ( !Gizmo.IsSelected )
			return;

		Gizmo.Draw.LineThickness = 6;
		Gizmo.Draw.Color = Color.Green.WithAlpha( 0.3f );
		Gizmo.Draw.LineCircle( Vector3.Zero, Radius );
	}

	public override bool Emit( ParticleEffect target )
	{
		var len = Random.Shared.Float( 0, Radius );

		var angle = len * MathF.PI * 2.0f;
		//var pos = new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * Radius;
		var pos = Vector3.Forward * 0.1f;

		var rand = angle;
		pos += Vector3.Left * MathF.Sin( angle ) * len;
		pos += Vector3.Up * MathF.Cos( angle ) * len;

		var emitPos = pos;

		var p = target.Emit( Transform.World.PointToWorld( emitPos ) );
		p.Velocity = Transform.World.NormalToWorld( Vector3.Forward ) * p.Velocity.Length;


		return true;
	}
}
