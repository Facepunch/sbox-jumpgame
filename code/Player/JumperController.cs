
internal partial class JumperController : PawnController
{

	[Net, Predicted]
	public float TimeSinceJumpDown { get; set; }
	[Net, Predicted]
	public Angles TargetAngles { get; set; }

	Vector3 Mins => new Vector3( -16, -16, 0 );
	Vector3 Maxs => new Vector3( 16, 16, 72 );
	bool Grounded => GroundEntity.IsValid();
	float WalkSpeed => 200.0f;
	float GroundAngle => 40.0f;
	float Gravity => 800.0f;
	float StopSpeed => 100.0f;
	float GroundFriction => 10.0f;
	float GroundAcceleration => 150.0f;
	float TimeUntilMaxJump => 2.0f;
	float MaxJumpStrength => 825.0f;

	public Particles jumpeffect { get; private set; }

	public override void Simulate()
	{
		base.Simulate();

		CheckGrounded();

		if ( Grounded )
		{
			GroundMove();
			TryJump();

		}
		else
		{
			Velocity += Vector3.Down * Gravity * Time.Delta;
			TryBounce();
		}

		Rotation = Rotation.Slerp( Rotation, Rotation.From( TargetAngles ), 8f * Time.Delta );
		if ( jumpeffect != null)
		{
			jumpeffect.SetPosition( 0, Pawn.Position );
		}

		StepMove();
	}


	
	private void TryJump()
	{
		if ( Input.Down( InputButton.Jump ) )
		{
			SetTag( "ducked" );
			TimeSinceJumpDown += Time.Delta;
		}

		var jumpAlpha = TimeSinceJumpDown / TimeUntilMaxJump;

		if ( jumpAlpha >= 1 || ( !Input.Down( InputButton.Jump ) && jumpAlpha > 0 ) )
		{
			TimeSinceJumpDown = 0;

			jumpAlpha = Math.Min( 0.4f + jumpAlpha, 1.0f );
			jumpAlpha = ((int)(jumpAlpha * 10.0f)) / 10.0f;

			if ( GetWishVelocity().Length > 0 )
				Velocity = Rotation.Forward * jumpAlpha * MaxJumpStrength * .5f;

			Velocity = Velocity.WithZ( jumpAlpha * MaxJumpStrength );

			ClearGroundEntity();
			AddEvent( "jump" );

			if( Pawn is JumperPawn p )
			{
				p.TotalJumps++;
			}

			if ( Prediction.FirstTime )
			{
				Sound.FromEntity( "jumper.jump", Pawn ).SetPitch( 1.0f - (0.5f * jumpAlpha) );		
				jumpeffect = Particles.Create( "particles/player/jump/jumper.jump.vpcf" );
				Particles.Create( "particles/player/land/jumper.land.vpcf", Position );
			}
		}

		if ( !Input.Down( InputButton.Jump ) )
		{
			TimeSinceJumpDown = 0;
		}
	}

	private void TryBounce()
	{
		var tr = TraceBBox( Position, Position + Velocity * Time.Delta );
		if ( !tr.Hit || tr.Normal.Angle( Vector3.Up ) < 80.0f ) return;

		var bounce = -tr.Normal * Velocity.Dot( tr.Normal );
		Velocity = ClipVelocity( Velocity, tr.Normal );
		Velocity += bounce;
		TargetAngles = Rotation.LookAt( tr.Normal ).Angles();

		if ( Host.IsServer || Prediction.FirstTime )
		{
			var hiteffect = Particles.Create( "particles/player/impact/jumper.impact.wall.vpcf", tr.EndPosition );
			hiteffect.SetForward( 0, tr.Normal );
			Sound.FromEntity( "jumper.impact.wall", Pawn );
		}
	}

	private void GroundMove()
	{
		var wishVel = GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;

		if ( wishdir.Length > 0 && GroundEntity != null )
		{
			TargetAngles = Rotation.LookAt( wishdir ).Angles();
		}

		TargetAngles = TargetAngles.WithPitch( 0 ).WithRoll( 0 );

		if ( TimeSinceJumpDown > 0 )
		{
			Velocity = 0;
			return;
		}

		Velocity = Velocity.WithZ( 0 );
		Velocity += BaseVelocity;
		ApplyFriction( StopSpeed, GroundFriction );
		Accelerate( wishdir, wishspeed, 0, GroundAcceleration );
	}

	private void CheckGrounded()
	{
		var pm = TraceBBox( Position, Position + Vector3.Down * 4.0f, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			var dot = Velocity.Dot( pm.Normal );
			
			if( dot < 0 )
			{
				Velocity = 0;
			}
		}
		else
		{
			if( !Grounded )
			{
				AddEvent( "landed" );

				if ( Host.IsServer || Prediction.FirstTime )
					Sound.FromWorld( "player.land", Position );

				if( GetFallDamage( Velocity.z ) > 0 )
				{
					DoFall();
				}
			}
			SetGroundEntity( pm.Entity );
		}
	}


	void DoFall()
	{
		if ( Pawn is not JumperPawn p ) 
			return;

		p.TotalFalls++;

		if ( Prediction.FirstTime || Host.IsServer )
		{
			Particles.Create( "particles/player/jump/jumper.jump.vpcf", Position );
			if ( GetFallDamage( Velocity.z ) >= 2 )
			{
				JumperGame.SendTease( GetRandomFallMessage() );
			}
		}
	}

	Vector3 GetWishVelocity( bool zeroPitch = false )
	{
		var result = new Vector3( Input.Forward, Input.Left, 0 );
		var inSpeed = result.Length.Clamp( 0, 1 );
		result *= Input.Rotation;

		if ( zeroPitch )
			result.z = 0;

		result = result.Normal * inSpeed;
		result *= WalkSpeed;

		return result;
	}

	void StepMove( float groundAngle = 46f, float stepSize = 18f )
	{
		MoveHelper mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn ).WithoutTags( "JumpPlayer" ); ;
		mover.MaxStandableAngle = groundAngle;
		mover.TryMoveWithStep( Time.Delta, stepSize );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
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

	void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
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

	void ClearGroundEntity()
	{
		if ( GroundEntity == null ) return;

		GroundEntity = null;
		GroundNormal = Vector3.Up;	
	}

	void SetGroundEntity( Entity entity )
	{
		GroundEntity = entity;

		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
			BaseVelocity = GroundEntity.Velocity;
		}
	}

	TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Mins, Maxs, liftFeet );
	}

	Vector3 TraceOffset;
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( Pawn )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - (norm * backoff);

		// garry: I don't totally understand how we could still
		//		  be travelling towards the norm, but the hl2 code
		//		  does another check here, so we're going to too.
		var adjust = Vector3.Dot( o, norm );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= norm * adjust;

		return o;
	}

	private int GetFallDamage( float fallspeed )
	{
		fallspeed = Math.Abs( fallspeed );

		if ( fallspeed < 700 ) return 0;
		if ( fallspeed < 1000 ) return 1;
		if ( fallspeed < 1300 ) return 2;
		if ( fallspeed < 1600 ) return 3;

		return 4;
	}

	private int lastFallMessage;
	private string GetRandomFallMessage()
	{
		var idx = Rand.Int( 0, fallMessages.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Rand.Int( 0, fallMessages.Count - 1 );

		lastFallMessage = idx;
		return string.Format( fallMessages[idx] );
	}

	private List<string> fallMessages = new()
	{
		"Thats a big fall!!",
		"Try not to fall so much next time!",
		"Ouch! That looked painful!",
		"Are you ok?",
		"What a fall!",
		"Don't fall again!",
		"Don't give up!",
		"Keep trying!",
		"Try again!",
		"It's like starting a new book...",
		"One day you will be a winner!",
		"One day you will look back to this and ask why...",
		"Try to be more careful next time!",
		"Where is your parachute?!",
		"Can't you fly?!",
		"Where are your wings?!",
		"Do you like falling?!",
		"And you call yourself a jumper?!",
		"And where do you think you are going?!",
		"Please don't fall again!",
		"You remind me of a cat!",
		"Try to visit a doctor!",
		"Pain is temporary, glory is forever!",
		"Peddle to the metal!",
		"Uh oh!",
		"Uh that wasn't good!",
		"When can we expect you to be back?",
		"When can we provide you with a new body?",
		"When can we process your insurance claim?",
		"One small step for man, one giant fall for mankind!",
		"It's a new day!"

	};
}
