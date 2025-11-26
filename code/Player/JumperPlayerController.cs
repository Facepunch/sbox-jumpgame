using Sandbox;
using Sandbox.UI;

using Sandbox.Services;

public class JumperPlayerController : Component
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	public Vector3 WishVelocity { get; set; }

	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public JumperCitizenAnimation AnimationHelper { get; set; }

	[Property] public JumperCharacterController CharacterController { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool IsRunning { get; set; }

	//JumperLogic
	Vector3 Mins => new Vector3( -16, -16, 0 );
	Vector3 Maxs => new Vector3( 16, 16, 62 );
	public float TimeSinceJumpDown { get; set; }
	public float TimeUntilMaxJump => 2.0f;
	float MaxJumpStrength => 885.0f;
	[Sync] public Angles TargetAngles { get; set; }
	private bool HasLanded { get; set; } = false;
	[Property] JumperPlayerStuff PlayerStats { get; set; }
	[Property] GameObject HitEffect { get; set; }
	[Property] GameObject JumpEffect { get; set; }
	[Property] GameObject TrailEffect { get; set; }

	bool CanJump;

	Vector3 LastGroundedPos { get; set; }
	int BounceCount { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
			return;

		LastGroundedPos = GameObject.Transform.Position;
	}


	protected override void OnUpdate()
	{
		// Eye input
		if ( !IsProxy )
		{
			var e = EyeAngles;
			e += Input.AnalogLook;
			e.pitch = e.pitch.Clamp( -89, 89 );
			e.roll = 0;
			//EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
			//EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
			//EyeAngles.roll = 0;

			//EyeAngles.pitch = Math.Clamp( EyeAngles.pitch, -44.9f, 75.9f );

			EyeAngles = e;

			IsRunning = Input.Down( "Duck" );
		}

		// rotate body to look angles
		if ( Body is not null )
		{
			if ( CharacterController.IsOnGround )
			{
				if ( WishVelocity.Length > 0 )
				{
					TargetAngles = Rotation.LookAt( WishVelocity ).Angles().WithRoll( 0 );
				}

				TargetAngles = TargetAngles.WithRoll( 0 );
			}

			Body.Transform.Rotation = Rotation.Slerp( Body.Transform.Rotation, Rotation.From( TargetAngles ), 8f * Time.Delta );
		}

		if ( AnimationHelper.IsValid() )
		{
			AnimationHelper.WithVelocity( CharacterController.Velocity );
			AnimationHelper.IsGrounded = CharacterController.IsOnGround;
			AnimationHelper.MoveRotationSpeed = 0;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = !IsRunning ? JumperCitizenAnimation.MoveStyles.Run : JumperCitizenAnimation.MoveStyles.Walk;
		}
	}

	TimeSince LastSave;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( PlayerStats != null )
		{
			PlayerStats.TimePlayed += Time.Delta;
		}
		
		BuildWishVelocity();

		if ( CharacterController.IsOnGround )
		{
			TryJump();

			if( LastSave > 7)
			{
				PlayerStats?.SaveStats();
				LastSave = 0;
			}
			LastGroundedPos = GameObject.Transform.Position;
			BounceCount = 0;
		}
		else
		{
			TryBounce();
		}

		if ( TimeSinceJumpDown > 0 )
		{
			CharacterController.Velocity = 0;
			return;
		}
		else if ( TimeSinceJumpDown > 0 )
		{
			CharacterController.ApplyFriction( 100, 10 );
			return;
		}
		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( IsRunning ? 4 : 10 );
		}
		else
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			//cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			CharacterController.ApplyFriction( 0.1f );
		}

		CharacterController.Move();

		//Gravity
		if ( !CharacterController.IsOnGround )
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
	}

	[Broadcast]
	public void OnJump( float floatValue, string dataString, object[] objects, Vector3 position )
	{
		AnimationHelper?.TriggerJump();
	}

	public void TryJump()
	{

		if ( Input.Down( "reload" ) )
		{
			TimeSinceJumpDown = 0;
			CanJump = false;

			return;
		}

		if ( Input.Down( "jump" ) && CanJump )
		{
			TimeSinceJumpDown += Time.Delta;
			var alpha = TimeSinceJumpDown.LerpInverse( 0, TimeUntilMaxJump );
			AnimationHelper.DuckLevel = alpha;
		}

		var jumpAlpha = TimeSinceJumpDown / TimeUntilMaxJump;

		if ( jumpAlpha >= 1 || (!Input.Down( "jump" ) && jumpAlpha > 0) )
		{
			TimeSinceJumpDown = 0;

			jumpAlpha = Math.Min( 0.4f + jumpAlpha, 1.0f );
			jumpAlpha = ((int)(jumpAlpha * 10.0f)) / 10.0f;

			if ( WishVelocity.Length > 0 )
				CharacterController.Velocity = Body.Transform.Rotation.Forward * jumpAlpha * MaxJumpStrength * 0.5f;

			CharacterController.Velocity = CharacterController.Velocity.WithZ( jumpAlpha * MaxJumpStrength );
			CharacterController.IsOnGround = false;
			var effect = JumpEffect.Clone();
			effect.Transform.Position = Transform.Position;
			effect.Transform.Rotation = Rotation.LookAt( CharacterController.Velocity );
			var snd = Sound.Play( "jumper.jump", GameObject.Transform.Position );
			snd.Pitch = 1.0f - (0.5f * jumpAlpha) * 1.5f;

			//var _trailef = SceneUtility.Instantiate( TrailEffect, Vector3.Zero, Rotation.LookAt( cc.Velocity ) );
			//_trailef.Parent = GameObject;
			HasLanded = false;

			PlayerStats.TotalJumps++;

			AnimationHelper?.TriggerJump();

		}

		if ( !Input.Down( "jump" ) )
		{
			AnimationHelper.DuckLevel = 0;
			TimeSinceJumpDown = 0;
			CanJump = true;
		}
	}

	public void TryBounce()
	{
		var tr = TraceBBox( Body.Transform.Position, Body.Transform.Position + CharacterController.Velocity * Time.Delta );

		if ( !tr.Hit || tr.Normal.Angle( Vector3.Up ) < 80.0f ) return;

		var bounce = -tr.Normal * CharacterController.Velocity.Dot( tr.Normal );
		CharacterController.Velocity = ClipVelocity( CharacterController.Velocity, tr.Normal );
		CharacterController.Velocity += bounce;

		var bounceAngles = Rotation.LookAt( bounce ).Angles();
		TargetAngles = bounceAngles.WithRoll( 0f );

		var effect = HitEffect.Clone();
		effect.Transform.Position = tr.EndPosition;
		effect.Transform.Rotation = Rotation.LookAt( tr.Normal );

		//SceneUtility.Instantiate( HitEffect, Transform.Position + Vector3.Up * 32, Rotation.LookAt( tr.Normal ) );
		Sound.Play( "jumper.impact.wall", Transform.Position );

		BounceCount++;

		// if we bounce 5 times get achievement
		if ( BounceCount >= 5 )
		{
			GetAchievement( "bounce_5" );
		}
		if ( BounceCount >= 10 )
		{
			GetAchievement( "bounce_10" );
		}
	}

	public void TryWind( Vector3 winddir, float strength )
	{
		CharacterController.Velocity += winddir * strength;
	}

	PhysicsTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Mins, Maxs, liftFeet );
	}
	Vector3 TraceOffset;
	public virtual PhysicsTraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Scene.PhysicsWorld.Trace.Ray( start + TraceOffset * 10, end + TraceOffset )
					.Size( mins, maxs )

					.WithoutTags( "player", "trigger" )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	public void TriggerLandingEvent(float velocity)
	{
		if ( GetFallDamage( CharacterController.LastVelocity.z ) >= 2 )
		{
			var fallMessage = Components.Get<JumperFallMessage>( FindMode.InChildren );
			fallMessage.DisplayMessage( GetRandomFallMessage() );
			PlayerStats.TotalFalls++;
			HasLanded = true;
		}

		if( DistanceFell(LastGroundedPos, GameObject.Transform.Position ) > 5000 )
		{
			GetAchievement( "fall_5000" );
		}
	}
	public int GetFallDamage( float fallspeed )
	{
		fallspeed = Math.Abs( fallspeed );

		if ( fallspeed < 700 ) return 0;
		if ( fallspeed < 1000 ) return 1;
		if ( fallspeed < 1300 ) return 2;
		if ( fallspeed < 1600 ) return 3;

		return 4;
	}
	public void BuildWishVelocity()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = 0;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "duck" ) ) WishVelocity *= 100.0f;
		else WishVelocity *= 200.0f;
	}
	Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - norm * backoff;

		var adjust = Vector3.Dot( o, norm );

		if ( adjust < 0.0f )
		{
			o -= norm * adjust;
		}

		return o;
	}

	float DistanceFell(Vector3 Start, Vector3 End )
	{
		return Math.Abs( Start.z - End.z );
	}
	public void Write( ref ByteStream stream )
	{
		stream.Write( IsRunning );
		stream.Write( EyeAngles );
	}
	public void Read( ByteStream stream )
	{
		IsRunning = stream.Read<bool>();
		EyeAngles = stream.Read<Angles>();
	}
	private int lastFallMessage;
	private string GetRandomFallMessage()
	{
		var idx = Game.Random.Int( 0, fallMessages.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Game.Random.Int( 0, fallMessages.Count - 1 );

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
		"It's a new day!",
		"Hello darkness my old friend!",
		"Could you please stop falling?",
		"Do you want to be a winner?",
		"Are you a winner?",
		"Are you a loser?",
		"Are you a winner or a loser?",
		"Damn it!",
		"Damn it all!",
		"Damn it all to hell!",
		"Damn it all to hell and back!",
		"Damn it all to hell and back again!",
		"Use the jump button to jump!",
		"I'll scratch your back if you scratch mine!",
		"I'll be back!",
		"Awoooo awoooo awoooo!",
		"3 bags of rice,5 carrots and 2 apples!"
	};

	void GetAchievement(string cheevo)
	{
		if ( IsProxy )
			return;

		Achievements.Unlock( cheevo );

		Log.Info($"Unlocked achievement: {cheevo}" );
	}
}
