
using Sandbox;

namespace JumpingSausage
{
	public class JumpingSausageCamera : CameraMode
	{

		private float distance = 250.0f;

		public float MinDistance => 100.0f;
		public float MaxDistance => 350.0f;
		public float DistanceStep => 60.0f;

		public override void Update()
		{
			var pawn = Local.Pawn as JumpingSausagePawn;

			if ( pawn == null ) return;

			UpdateViewBlockers( pawn );

			var center = pawn.Position + Vector3.Up * 76;
			//var distance = 150.0f * pawn.Scale;
			var targetPos = center + Input.Rotation.Forward * -distance;

			var tr = Trace.Ray( center, targetPos )
				.Ignore( pawn )
				.Radius( 8 )
				.Run();

			var endpos = tr.EndPosition;


			Position = endpos;
			Rotation = Input.Rotation;

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
		}

		public override void Deactivated()
		{
			base.Deactivated();
		}

		private void UpdateViewBlockers( JumpingSausagePawn pawn )
		{
			var traces = Trace.Sphere( 3f, CurrentView.Position, pawn.Position + Vector3.Up * 16 ).RunAll();

			if ( traces == null ) return;
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

			if ( Input.MouseWheel != 0 )
			{
				distance = distance.LerpTo( distance - Input.MouseWheel * DistanceStep, Time.Delta * 10, true ).Clamp( MinDistance, MaxDistance );
			}
		}

	}
}
