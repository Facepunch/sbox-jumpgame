using Sandbox;

namespace JumpingSausage.Movement
{
	class Walk : BaseMoveMechanic
	{

		public float StopSpeed => 100.0f;
		public float StepSize => 18.0f;
		public float GroundAngle => 44.0f;
		public bool AutoJump => false;
		public float DefaultSpeed => 2000f;
		public float WalkSpeed => 2000f;
		public float GroundFriction => 400.0f;
		public float MaxNonJumpVelocity => 140.0f;
		public float SurfaceFriction { get; set; } = 1f;
		public float Acceleration => 4.5f;
		public float DuckAcceleration => 5f;

		public override bool AlwaysSimulate => true;

		private TimeUntil Coyote;

		public Walk( JumpingSausageController controller )
			: base( controller )
		{

		}

		public override void Simulate()
		{
			if ( ctrl.GroundEntity != null )
				WalkMove();

			CheckJumpButton();
		}

		private bool prevGrounded;
		public override void PreSimulate()
		{
			base.PreSimulate();

			prevGrounded = ctrl.GroundEntity.IsValid();
		}

		public override void PostSimulate()
		{
			base.PostSimulate();

			CategorizePosition( ctrl.GroundEntity != null );
			UpdateBaseVelocity();
		}

		public override float GetWishSpeed()
		{
			if ( InputActions.Walk.Down() ) return WalkSpeed;

			return DefaultSpeed;
		}

		private void WalkMove()
		{
			if ( ctrl.IsAboutToJump )
				return;
			if ( ctrl.GroundEntity == null )
				return;
			
			var wishVel = ctrl.GetWishVelocity( true );
			var wishdir = wishVel.Normal;
			var wishspeed = wishVel.Length;

			// todo: might wanna keep things contained...
			var ducker = ctrl.GetMechanic<Ducker>();
			var friction = GroundFriction * SurfaceFriction;
			bool ducking = ducker != null && ducker.IsActive;

			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ctrl.ApplyFriction( StopSpeed, friction );

			var accel = ducking ? DuckAcceleration : Acceleration;
			accel += GetMomentum();

			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ctrl.Accelerate( wishdir, wishspeed, 0, accel );
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );

			// Add in any base velocity to the current velocity.
			ctrl.Velocity += ctrl.BaseVelocity;

			try
			{
				if ( ctrl.Velocity.Length < 1.0f )
				{
					ctrl.Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination
				var dest = (ctrl.Position + ctrl.Velocity * Time.Delta).WithZ( ctrl.Position.z );

				var pm = ctrl.TraceBBox( ctrl.Position, dest );

				if ( pm.Fraction == 1 )
				{
					ctrl.Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				ctrl.StepMove();
			}
			finally
			{

				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				ctrl.Velocity -= ctrl.BaseVelocity;
			}

			StayOnGround();
		}

		private void CheckJumpButton()
		{

		}

		private Transform LastGroundTx;
		private Entity LastGroundEntity;
		private void UpdateBaseVelocity()
		{
			var prevtx = LastGroundTx;
			var prevground = LastGroundEntity;
			LastGroundEntity = ctrl.GroundEntity;
			LastGroundTx = ctrl.GroundEntity?.Transform ?? default;
			ctrl.BaseVelocity = 0;

			if ( ctrl.GroundEntity == null ) return;
			if ( ctrl.GroundEntity != prevground ) return;
			if ( prevtx == ctrl.GroundEntity.Transform ) return;

			var newPosition = ctrl.Position;

			// just trying to guess BaseVelocity based on transform delta of ground we're standing on
			// todo: fix platforms so all this can be removed
			if ( prevtx.Rotation != ctrl.GroundEntity.Transform.Rotation )
			{
				var rotdelta = Rotation.Difference( prevtx.Rotation, ctrl.GroundEntity.Rotation );
				//newPosition = newPosition.RotateAroundPivot( ctrl.GroundEntity.Position, rotdelta );
			}

			var posdelta = ctrl.GroundEntity.Position - prevtx.Position;
			if ( posdelta.z < 0 ) posdelta.z = 0; // don't really need to move down, gravity handles that
			newPosition += posdelta;

			ctrl.Position = ctrl.Position.WithZ( newPosition.z ); // but if we're moving up we need to go with it
			ctrl.BaseVelocity = (newPosition - ctrl.Position) / Time.Delta;
		}

		// todo: really need to do this in a way we can define simply
		// how long it takes to go from standing to max speed
		private float GetMomentum()
		{
			var a = ctrl.Velocity.WithZ( 0 ).Length / DefaultSpeed;
			a = Easing.EaseIn( a );
			return a * 2.4f;
		}

		/// <summary>
		/// Try to keep a walking player on the ground when running down slopes etc
		/// </summary>
		private void StayOnGround()
		{
			var start = ctrl.Position + Vector3.Up * 2;
			var end = ctrl.Position + Vector3.Down * StepSize;

			// See how far up we can go without getting stuck
			var trace = ctrl.TraceBBox( ctrl.Position, start );
			start = trace.EndPosition;

			// Now trace down from a known safe position
			trace = ctrl.TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;
			// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
			// float flDelta = fabs( mv->GetAbsOrigin().z - trace.m_vEndPos.z );
			// if ( flDelta > 0.5f * DIST_EPSILON )

			ctrl.Position = trace.EndPosition;
		}

		private void CategorizePosition( bool bStayOnGround )
		{
			// Doing this before we move may introduce a potential latency in water detection, but
			// doing it after can get us stuck on the bottom in water if the amount we move up
			// is less than the 1 pixel 'threshold' we're about to snap to.	Also, we'll call
			// this several times per frame, so we really need to avoid sticking to the bottom of
			// water on each call, and the converse case will correct itself if called twice.
			//CheckWater();

			var point = ctrl.Position - Vector3.Up * 2;
			var vBumpOrigin = ctrl.Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			bool bMovingUpRapidly = ctrl.Velocity.z > MaxNonJumpVelocity;
			bool bMoveToEndPos = false;

			if ( ctrl.GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly )
			{
				ctrl.ClearGroundEntity();
				return;
			}

			var pm = ctrl.TraceBBox( vBumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
			{
				ctrl.ClearGroundEntity();
				bMoveToEndPos = false;
			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				ctrl.Position = pm.EndPosition;
			}
		}

		private void UpdateGroundEntity( TraceResult tr )
		{
			ctrl.GroundNormal = tr.Normal;

			// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
			// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
			// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
			SurfaceFriction = tr.Surface.Friction * 1.25f;
			if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

			ctrl.SetGroundEntity( tr.Entity );
		}

	}
}
