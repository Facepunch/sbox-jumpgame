using Sandbox;
[Title( "Jumper - Wind" )]
[Category( "Jumper" )]
[Icon( "wind_power" )]

public sealed class JumperWindTunnel : Component, Component.ITriggerListener
{
	[Property] public float WindGroundedStrength { get; set; } = 25.0f;
	[Property] public float WindAirStrength { get; set; } = 15.0f;


	protected override void DrawGizmos()
	{

		Gizmo.Draw.LineThickness = 10;
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.Line( 0, 0 + Vector3.Forward * 100);
	}

	List<GameObject> Players = new();

	protected override void OnUpdate()
	{
		if ( Players is null )
			return;

		foreach ( var player in Players )
		{
			var plycomp = player.GetComponent<PlayerController>();
			if( plycomp.IsOnGround)
			{
				//plycomp.TryWind( Transform.Rotation.Forward, WindGroundedStrength );
				plycomp.Body.Velocity += WorldRotation.Forward * WindGroundedStrength;
			}
			else
			{
				plycomp.Body.Velocity += WorldRotation.Forward * WindAirStrength;
				//plycomp.TryWind( Transform.Rotation.Forward, WindAirStrength );
			}

		}
	}
	
	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Root.Tags.Has( "player" ) )
		{
			var ply = other.GameObject.Root;
			var plyComp = ply.GetComponent<PlayerController>();
			if ( plyComp.IsValid() )
			{
				plyComp.PreventGrounding( 0.2f );
				plyComp.Body.Velocity += WorldRotation.Forward * 15;
				//plyComp.TryWind( Transform.Rotation.Forward, 15 );
				Players.Add( ply );
			}
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			var ply = other.GameObject.Parent;

			Players.Remove( ply );
		}

	}
}
