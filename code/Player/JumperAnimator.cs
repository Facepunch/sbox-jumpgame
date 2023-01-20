
public class JumperAnimator 
{

	private JumperPawn Pawn;

	internal JumperAnimator( JumperPawn p )
	{
		Pawn = p;
	}

	TimeSince TimeSinceFootShuffle = 60;

	public bool LookAtMe;

	public void Simulate()
	{
		if ( !Pawn.IsValid() ) return;

		var idealRotation = Pawn.Rotation;

		DoRotation( idealRotation );
		DoWalk();

		//
		// Let the animation graph know some shit
		//
		bool sitting = Pawn.Tags.Has( "sitting" );
		bool noclip = Pawn.Tags.Has( "noclip" ) && !sitting;
		bool skidding = Pawn.Tags.Has( "skidding" );

		Pawn.SetAnimParameter( "b_grounded", Pawn.GroundEntity != null || noclip || sitting );
		Pawn.SetAnimParameter( "b_noclip", noclip );
		Pawn.SetAnimParameter( "b_sit", sitting );
		Pawn.SetAnimParameter( "skid", skidding ? 1.0f : 0f );
		Pawn.SetAnimParameter( "b_swim", Pawn.GetWaterLevel() > 0.5f && !sitting );

		if ( Game.IsClient && Pawn.Client.IsValid() )
		{
			Pawn.SetAnimParameter( "voice",Pawn.Client.Voice.LastHeard < 0.5f ? Pawn.Client.Voice.CurrentLevel : 0.0f );
		}

		if ( LookAtMe )
		{
			Vector3 aimPos = Pawn.EyePosition + Pawn.Rotation.Forward * 200;
			Vector3 lookPos = aimPos;

			//
			// Look in the direction what the player's input is facing
			//
			Pawn.SetAnimLookAt( "aim_eyes", Pawn.EyePosition, lookPos );
			Pawn.SetAnimLookAt( "aim_head", Pawn.EyePosition, lookPos );
			Pawn.SetAnimLookAt( "aim_body", Pawn.EyePosition, aimPos );
		}

		if ( Input.Down( InputButton.Jump ) )
		{
			if ( Pawn.Controller is JumperController ctrl )
			{
				var alpha = ctrl.TimeSinceJumpDown.LerpInverse( 0, ctrl.TimeUntilMaxJump );
				Pawn.SetAnimParameter( "duck", alpha );
			}
			else
			{
				Pawn.SetAnimParameter( "duck", 1.0f );
			}
		}
		else
		{
			Pawn.SetAnimParameter( "duck", 0 );
		}
	}

	public virtual void DoRotation( Rotation idealRotation )
	{
		//
		// Our ideal player model rotation is the way we're facing
		//
		var allowYawDiff = Pawn?.ActiveChild == null ? 90 : 50;

		float turnSpeed = 0.01f;
		if ( Pawn.Tags.Has( "ducked" ) ) turnSpeed = 0.1f;

		//
		// If we're moving, rotate to our ideal rotation
		//
		Pawn.Rotation = Rotation.Slerp( Pawn.Rotation, idealRotation, Pawn.Velocity.Length * Time.Delta * turnSpeed );

		//
		// Clamp the foot rotation to within 120 degrees of the ideal rotation
		//
		Pawn.Rotation = Pawn.Rotation.Clamp( idealRotation, allowYawDiff, out var change );

		//
		// If we did restrict, and are standing still, add a foot shuffle
		//
		if ( change > 1 && Pawn.Velocity.Length <= 1 ) TimeSinceFootShuffle = 0;

		Pawn.SetAnimParameter( "b_shuffle", TimeSinceFootShuffle < 0.1 );
	}

	void DoWalk()
	{
		// Move Speed
		{
			var dir = Pawn.Velocity;
			var forward = Pawn.Rotation.Forward.Dot( dir );
			var sideward = Pawn.Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			Pawn.SetAnimParameter( "move_direction", angle );
			Pawn.SetAnimParameter( "move_speed", Pawn.Velocity.Length );
			Pawn.SetAnimParameter( "move_groundspeed", Pawn.Velocity.WithZ( 0 ).Length );
			Pawn.SetAnimParameter( "move_y", sideward );
			Pawn.SetAnimParameter( "move_x", forward );
			Pawn.SetAnimParameter( "move_z", Pawn.Velocity.z );
		}

		// Wish Speed
		{
			var dir = Pawn.Velocity;
			var forward = Pawn.Rotation.Forward.Dot( dir );
			var sideward = Pawn.Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			Pawn.SetAnimParameter( "wish_direction", angle );
			Pawn.SetAnimParameter( "wish_speed", Pawn.Velocity.Length );
			Pawn.SetAnimParameter( "wish_groundspeed", Pawn.Velocity.WithZ( 0 ).Length );
			Pawn.SetAnimParameter( "wish_y", sideward );
			Pawn.SetAnimParameter( "wish_x", forward );
			Pawn.SetAnimParameter( "wish_z", Pawn.Velocity.z );
		}
	}
}
