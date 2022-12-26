
public class JumperCamera
{

	private float distance;
	
	private Vector3 targetPosition;
	public float ZoomLevel { get; set; } = 250f;

	public float MinDistance => 120.0f;
	public float MaxDistance => 350.0f;
	public float DistanceStep => 60.0f;

	public void Update()
	{
		if ( Game.LocalPawn is not JumperPawn pawn )
			return;

		ZoomLevel += -Input.MouseWheel * Time.Delta * 1000f;
		ZoomLevel = ZoomLevel.Clamp( MinDistance, MaxDistance );

		var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
		distance = distance.LerpTo( ZoomLevel, 5f * Time.Delta );
		targetPosition = Vector3.Lerp( targetPosition, pawn.Position, 8f * Time.Delta );

		var playerRotation = pawn.ViewAngles.ToRotation();

		var height = 48f.LerpTo( 96f, distanceA );
		var center = targetPosition + Vector3.Up * height + playerRotation.Backward * 8f;
		var targetPos = center + playerRotation.Backward * ZoomLevel;

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
}
