using Sandbox;

namespace JumpingSausage
{
	public class JumpingSausageShiftCamera : CameraMode
	{

		private float distance;
		private float targetDistance = 250f;
		private Vector3 targetPosition;
		private Angles currentForward;

		public float MinDistance => 120.0f;
		public float MaxDistance => 350.0f;
		public float DistanceStep => 60.0f;

		public override void Update()
		{
			var pawn = Local.Pawn as JumpingSausagePawn;

			if ( pawn == null ) return;

			if ( pawn.Velocity.WithZ( 0 ).Length > 0 )
			{
				var targetFwd = pawn.Rotation.Angles();
				currentForward = Angles.Lerp( currentForward, targetFwd, .1f * Time.Delta );
			}

			if ( !CameraAdjustment.AlmostEqual( 0f ) )
			{
				var amount = CameraAdjustment * 10f * Time.Delta;
				CameraAdjustment = CameraAdjustment.LerpTo( 0, 10f * Time.Delta );
				currentForward.yaw += amount;
			}

			distance = distance.LerpTo( targetDistance, 5f * Time.Delta );
			targetPosition = Vector3.Lerp( targetPosition, pawn.Position, 8f * Time.Delta );

			var distanceA = distance.LerpInverse( MinDistance, MaxDistance );
			var height = 48f.LerpTo( 128f, distanceA );
			var center = targetPosition + Vector3.Up * height;
			var targetPos = center + Input.Rotation.Forward * -distance;

			var tr = Trace.Ray( center, targetPos )
				.Ignore( pawn )
				.Radius( 8 )
				.Run();

			var endpos = tr.EndPosition;

			Position = endpos;
			Rotation = Input.Rotation;
			Rotation *= Rotation.FromPitch( distanceA * 10f );

			var rot = pawn.Rotation.Angles() * .015f;
			rot.yaw = 0;

			Rotation *= Rotation.From( rot );

			var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
			var fov = 70f.LerpTo( 80f, spd );

			FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );

			Viewer = null;
		}

		public override void Activated()
		{
			base.Activated();

			FieldOfView = 70;
			targetPosition = Local.Pawn.Position;
		}

		private float CameraAdjustment;
		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

			if ( InputActions.Menu.Pressed() )
				CameraAdjustment = -60;
			if ( InputActions.Use.Pressed() )
				CameraAdjustment = 60;

			input.ViewAngles = currentForward;

			if ( Input.MouseWheel != 0 )
			{
				targetDistance += -Input.MouseWheel * DistanceStep;
				targetDistance = targetDistance.Clamp( MinDistance, MaxDistance );
			}
		}

	}
}
