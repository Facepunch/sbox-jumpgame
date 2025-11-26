
public sealed class JumperCamera : Component
{
	//public CameraComponent CameraObject { get; set; }
	//[Property] public GameObject PlayerObject { get; set; }

	private Vector3 targetPosition;

	private float distance;
	public float ZoomLevel { get; set; } = 250f;
	public float MinDistance => 120.0f;
	public float MaxDistance => 350.0f;
	public float DistanceStep => 60.0f;

	protected override void OnPreRender()
	{
		if ( IsProxy )
			return;

		UpdateCamera();

		UpdateRenderAlpha();
	}


	void UpdateCamera()
	{
		var CameraObject = Scene.GetAllComponents<CameraComponent>().Where( x => x.IsMainCamera ).FirstOrDefault();

		if ( CameraObject is null ) return;

		if ( CameraObject.IsValid())
		{
			ZoomLevel += -Input.MouseWheel.y * RealTime.Delta * 1000.0f;
			ZoomLevel = ZoomLevel.Clamp( MinDistance, MaxDistance );

			var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
			distance = distance.LerpTo( ZoomLevel, 5.0f * RealTime.Delta );
			targetPosition = Vector3.Lerp( targetPosition, GameObject.WorldPosition, 8.0f * RealTime.Delta );

			var pc = GameObject.Components.Get<PlayerController>( FindMode.EnabledInSelfAndChildren );
			if ( !pc.IsValid() ) return;
			var playerRotation = pc.EyeAngles.ToRotation();

			var height = 48.0f.LerpTo( 96.0f, distanceA );
			var center = targetPosition + Vector3.Up * height + playerRotation.Backward * 8.0f;
			var targetPos = center + playerRotation.Backward * ZoomLevel;

			var tr = Scene.PhysicsWorld.Trace.Ray( center, targetPos )
				.WithoutTags( "trigger", "player" )
				.Radius( 8.0f )
				.Run();

			if ( tr.Hit )
			{
				distance = Math.Min( distance, tr.Distance );
			}

			CameraObject.WorldPosition = center + playerRotation.Backward * distance;
			CameraObject.WorldRotation = playerRotation;
			CameraObject.WorldRotation *= Rotation.FromPitch( distanceA * 10.0f );
		}
	}
	public const float MaxRenderDistanceOther = 256.0f;
	public const float MaxRenderDistanceSelf = 200.0f;
	private void UpdateRenderAlpha()
	{
		var CameraObject = Scene.GetAllComponents<CameraComponent>().Where( x => x.IsMainCamera ).FirstOrDefault();

		var dist = CameraObject.WorldPosition.Distance( GameObject.WorldPosition );
		var a = 1.0f - dist.LerpInverse( MaxRenderDistanceSelf, MaxRenderDistanceSelf * 0.1f );
		a = Math.Max( a, .15f );
		a = Sandbox.Utility.Easing.EaseOut( a );

		var render = GameObject.Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants );

		foreach ( var item in render )
		{
			item.Tint = item.Tint.WithAlpha( a );
		}
	}
}
