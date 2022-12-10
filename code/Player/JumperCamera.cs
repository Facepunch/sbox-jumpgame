
public class JumperCamera
{

	private float distance;
	private float targetDistance = 250f;
	private Vector3 targetPosition;

	public float MinDistance => 120.0f;
	public float MaxDistance => 350.0f;
	public float DistanceStep => 60.0f;

	public void Update()
	{
		if ( Game.LocalPawn is not JumperPawn pawn )
			return;

		var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
		distance = distance.LerpTo( targetDistance, 5f * Time.Delta );
		targetPosition = Vector3.Lerp( targetPosition, pawn.Position, 8f * Time.Delta );

		var playerRotation = pawn.ViewAngles.ToRotation();

		var height = 48f.LerpTo( 96f, distanceA );
		var center = targetPosition + Vector3.Up * height + playerRotation.Backward * 8f;
		var targetPos = center + playerRotation.Backward * targetDistance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.WithAnyTags( "world", "solid" )
			.WithoutTags("player" )
			.Radius( 8 )
			.Run();

		if ( tr.Hit )
		{
			distance = Math.Min( distance, tr.Distance );
		}

		Camera.Position = center + playerRotation.Backward * distance;
		Camera.Rotation = playerRotation;
		Camera.Rotation *= Rotation.FromPitch( distanceA * 10f );

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 70f.LerpTo( 80f, spd );

		Camera.FieldOfView = 90f;
		Camera.ZNear = 6;
		Camera.FirstPersonViewer = null;
	}

	[Event.Client.BuildInput]
	public void BuildInput()
	{
		if ( Input.MouseWheel != 0 )
		{
			targetDistance += -Input.MouseWheel * DistanceStep;
			targetDistance = targetDistance.Clamp( MinDistance, MaxDistance );
		}
	}

}
