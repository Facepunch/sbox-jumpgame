using Sandbox;
using System;

namespace JumpingSausage
{
	public class JumpingSausageOrbitCamera : CameraMode
	{

		private float distance;
		private float targetDistance = 250f;
		private Vector3 targetPosition;

		public float MinDistance => 120.0f;
		public float MaxDistance => 350.0f;
		public float DistanceStep => 60.0f;

		public Vector3 ViewNormal;
		public Vector3 LastPosition;

		public Rotation LastCameraRotation;

		public override void Update()
		{
			var pawn = Local.Pawn as JumpingSausagePawn;

			if ( pawn == null ) return;

			UpdateViewBlockers( pawn );
			var distanceA = distance.LerpInverse( MinDistance, MaxDistance );

			distance = distance.LerpTo( targetDistance, 5f * Time.Delta );
			targetPosition = Vector3.Lerp( targetPosition, pawn.Position, 8f * Time.Delta );

			var height = 48f.LerpTo( 96f, distanceA );
			var center = targetPosition + Vector3.Up * height;
			center += Input.Rotation.Backward * 8f;
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

			var endpos = center + Input.Rotation.Backward * distance;

			Position = endpos;
			Rotation = Input.Rotation;
			Rotation *= Rotation.FromPitch( distanceA * 10f );

			var rot = pawn.Rotation.Angles() * .015f;
			rot.yaw = 0;

			Rotation *= Rotation.From( rot );

			var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
			var fov = 70f.LerpTo( 80f, spd );

			FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );
			ZNear = 6;
			Viewer = null;
		}

		public void CalculateTargetPosition()
		{
			// How far can our target can be from the player
			// Jump height works well enough for this
			float maxDistance = 50.0f;

			targetPosition += Local.Pawn.Velocity.WithZ( 0 ) * Time.Delta * 1.333f;

			// If player is rotating the camera manually, we should
			// Focus on center again
			float delta = LastCameraRotation.Distance( Input.Rotation );
			targetPosition = targetPosition.LerpTo( Local.Pawn.Position, delta * 0.01f );

			LastCameraRotation = Input.Rotation;

			// Clamp distance
			if ( Local.Pawn.Position.Distance( targetPosition ) > maxDistance )
			{
				Vector3 distanceNormal = (targetPosition - Local.Pawn.Position).Normal;
				targetPosition = Local.Pawn.Position + ( distanceNormal * maxDistance );
			}

		}

		public override void Activated()
		{
			base.Activated();

			FieldOfView = 70;
			targetPosition = Local.Pawn.Position;
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
				targetDistance += -Input.MouseWheel * DistanceStep;
				targetDistance = targetDistance.Clamp( MinDistance, MaxDistance );
			}
		}

	}
}
