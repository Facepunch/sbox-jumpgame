
public class JumperCamera : CameraMode
{

	private float distance;
	private float targetDistance = 250f;
	private Vector3 targetPosition;

	public float MinDistance => 120.0f;
	public float MaxDistance => 350.0f;
	public float DistanceStep => 60.0f;

	public override void Update()
	{
		if ( Local.Pawn is not JumperPawn pawn )
			return;

		var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
		distance = distance.LerpTo( targetDistance, 5f * Time.Delta );
		targetPosition = Vector3.Lerp( targetPosition, pawn.Position, 8f * Time.Delta );

		var height = 48f.LerpTo( 96f, distanceA );
		var center = targetPosition + Vector3.Up * height + Input.Rotation.Backward * 8f;
		var targetPos = center + Input.Rotation.Backward * targetDistance;

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

		Position = center + Input.Rotation.Backward * distance;
		Rotation = Input.Rotation;
		Rotation *= Rotation.FromPitch( distanceA * 10f );

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 70f.LerpTo( 80f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );
	}

	public override void Activated()
	{
		base.Activated();

		Viewer = null;
		ZNear = 6;
		FieldOfView = 70;
		targetPosition = Local.Pawn.Position;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( Input.MouseWheel != 0 )
		{
			targetDistance += -Input.MouseWheel * DistanceStep;
			targetDistance = targetDistance.Clamp( MinDistance, MaxDistance );
		}
	}

}
