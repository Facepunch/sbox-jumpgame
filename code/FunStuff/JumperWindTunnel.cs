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
			var plycomp = player.Components.Get<JumperPlayerController>();
			var cc = player.Components.Get<JumperCharacterController>();
			if(cc.IsOnGround)
			{
				plycomp.TryWind( Transform.Rotation.Forward, WindGroundedStrength );
			}
			else
			{
				plycomp.TryWind( Transform.Rotation.Forward, WindAirStrength );
			}

		}
	}
	
	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{

			var ply = other.GameObject.Parent;
			var plyComp = ply.Components.Get<JumperPlayerController>();
			
			plyComp.TryWind( Transform.Rotation.Forward, 15 );
			Players.Add( ply );
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
