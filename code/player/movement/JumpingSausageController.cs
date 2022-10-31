
using Sandbox;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JumpingSausage.Movement
{
	partial class JumpingSausageController : BasePlayerController
	{
		[Net] public bool IsHolding { get; set; }
		[Net] public bool IsAboutToJump { get; set; }
		[Net] public float EnergyDrain { get; set; } = 20f;
		[Net, Predicted] public Vector3 Impulse { get; set; }

		public float EyeHeight => 64.0f;
		public float BodyGirth => 32.0f;
		public float BodyHeight => 72.0f;
		public Vector3 Mins { get; private set; }
		public Vector3 Maxs { get; private set; }

		private List<BaseMoveMechanic> mechanics = new();
		private BaseMoveMechanic activeMechanic => mechanics.FirstOrDefault( x => x.IsActive );

		public IReadOnlyList<BaseMoveMechanic> Mechanics => mechanics;

		public JumpingSausageController()
		{
			mechanics.Add( new HoldJump( this ) );
			mechanics.Add( new PlayerBounce( this ) );
			mechanics.Add( new Walk( this ) );
			mechanics.Add( new AirMove( this ) );
			mechanics.Add( new WaterMove( this ) );
			mechanics.Add( new Unstucker( this ) );
			mechanics.Add( new Ducker( this ) );
			mechanics.Add( new GetPushed( this ) );
			
			//mechanics.Add( new LedgeGrab( this ) ); 
			//mechanics.Add( new WallJump( this ) ); Bit too buggy atm
			
			mechanics.Add( new MoveDebug( this ) );
		}

		// should be able to add and remove mechanics without worrying 
		// about something breaking, so this can be bad
		public T GetMechanic<T>() where T : BaseMoveMechanic
		{
			return mechanics.FirstOrDefault( x => x is T ) as T;
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			if ( GroundEntity != null )
			{
				EyeRotation = Input.Rotation;
			}	
		}

		public override void Simulate()
		{
			EyeLocalPosition = Vector3.Up * (64 * Pawn.Scale) + TraceOffset;
			EyeLocalPosition *= activeMechanic != null ? activeMechanic.EyePosMultiplier : 1f;
			
			if ( GroundEntity != null )
			{
				EyeRotation = Input.Rotation;
			}


				var wishdir = GetWishVelocity( true ).Normal;
			if(wishdir.Length > 0 && GroundEntity != null )
			{
				var targetRot = Rotation.LookAt( wishdir ).Angles().WithPitch( 0 ).WithRoll( 0 );
				Rotation = Rotation.Slerp( Rotation, Rotation.From( targetRot ), 8f * Time.Delta );
			}

			UpdateBBox();

			// This is confusing and needs review:
			//		PreSimulate and PostSimulate are always called if 
			//		the mechanic is active or AlwaysSimulate=true

			//		Simulate is only called if the mechanic is active, 
			//		AlwaysSimulates, AND there's no other mechanic in control

			//		The control is for things like vaulting, it stops 
			//		all other mechanics until its finished with the vault

			// Pros: modular, easy to edit/add movement mechanics

			if ( Impulse.Length > 0 )
			{
				ClearGroundEntity();
				Velocity += Impulse;
				Impulse = 0f;
			}

			foreach ( var m in mechanics )
			{
				if ( !m.IsActive && !m.AlwaysSimulate ) continue;
				m.PreSimulate();
			}

			var control = activeMechanic;

			if ( control == null )
			{
				foreach ( var m in mechanics )
				{
					// try to activate, i.e. vault looks for a ledge in front of the player
					if ( !m.Try() ) continue;
					control = m;
					break;
				}
			}

			if ( control != null && control.TakesOverControl )
			{
				control.Simulate();
			}
			else
			{
				foreach ( var m in mechanics )
				{
					if ( !m.IsActive && !m.AlwaysSimulate ) continue;
					m.Simulate();
				}
			}

			foreach ( var m in mechanics )
			{
				if ( !m.IsActive && !m.AlwaysSimulate ) continue;
				m.PostSimulate();
			}
		}	
		
		public virtual void SetBBox( Vector3 mins, Vector3 maxs )
		{
			Mins = mins;
			Maxs = maxs;
		}

		public virtual void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

			activeMechanic?.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

			SetBBox( mins, maxs );
		}

		public Vector3 GetWishVelocity( bool zeroPitch = false )
		{
			var result = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = result.Length.Clamp( 0, 1 );
			result *= Input.Rotation;

			if ( zeroPitch )
				result.z = 0;

			result = result.Normal * inSpeed;
			result *= GetWishSpeed();

			return result;
		}

		public virtual float GetWishSpeed()
		{
			var ws = -1f;
			if ( activeMechanic != null ) ws = activeMechanic.GetWishSpeed();
			if ( ws >= 0 ) return ws;

			return GetMechanic<Walk>().GetWishSpeed();
		}

		public void StepMove( float groundAngle = 46f, float stepSize = 18f )
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn ).WithoutTags( "Platplayer" ); ;
			mover.MaxStandableAngle = groundAngle;

			mover.TryMoveWithStep( Time.Delta, stepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public void Move( float groundAngle = 46f )
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn ).WithoutTags( "Platplayer" );
			mover.MaxStandableAngle = groundAngle;

			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
		{
			if ( speedLimit > 0 && wishspeed > speedLimit )
				wishspeed = speedLimit;

			var currentspeed = Velocity.Dot( wishdir );
			var addspeed = wishspeed - currentspeed;

			if ( addspeed <= 0 )
				return;

			var accelspeed = acceleration * Time.Delta * wishspeed;

			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			Velocity += wishdir * accelspeed;
		}

		public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
		{
			var speed = Velocity.Length;
			if ( speed < 0.1f ) return;

			var control = (speed < stopSpeed) ? stopSpeed : speed;
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;

			if ( newspeed != speed )
			{
				newspeed /= speed;
				Velocity *= newspeed;
			}
		}

		public void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
		}

		public void SetGroundEntity( Entity entity )
		{
			GroundEntity = entity;

			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
				BaseVelocity = GroundEntity.Velocity;

			}
		}

		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, Mins, Maxs, liftFeet );
		}
	}
}
