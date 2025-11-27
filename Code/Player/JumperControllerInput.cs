using Sandbox;
using Sandbox.Services;
using Sandbox.UI;
using System;
using static Sandbox.PlayerController;

public sealed partial class PlayerInput : Component
{
	[RequireComponent]
	public PlayerController Controller { get; set; }
	[Property] JumperPlayerStuff PlayerStats { get; set; }

	[Property, Feature( "Input" )] public float JumpSpeed { get; set; } = 300;
	[Property, Feature( "Input" )] public float WalkSpeed { get; set; } = 80;
	[Property, Feature( "Input" )] public float RunSpeed { get; set; } = 110;
	[Property, Feature( "Input" )] public float DuckedSpeed { get; set; } = 70;
	[Property, Feature( "Input" )] public float DuckedHeight { get; set; } = 36;

	/// <summary>
	/// Allows to player to interact with things by "use"ing them. 
	/// Usually by pressing the "use" button.
	/// </summary>
	[Property, Feature( "Input" ), ToggleGroup( "EnablePressing", Label = "Enable Pressing" )] public bool EnablePressing { get; set; } = true;

	/// <summary>
	/// The button that the player will press to use things
	/// </summary>
	[Property, Feature( "Input" ), Group( "EnablePressing" ), InputAction] public string UseButton { get; set; } = "use";

	/// <summary>
	/// How far from the eye can the player reach to use things
	/// </summary>
	[Property, Feature( "Input" ), Group( "EnablePressing" )] public float ReachLength { get; set; } = 130;

	/// <summary>
	/// Pitch clamp for the camera
	/// </summary>
	[Property, Feature( "Input" ), Category( "Eye Angles" ), Range( 0, 180 )] public float PitchClamp { get; set; } = 90;

	/// <summary>
	/// Allows modifying the eye angle sensitivity. Note that player preference sensitivity is already automatically applied, this is just extra.
	/// </summary>
	[Property, Feature( "Input" ), Category( "Eye Angles" ), Range( 0, 2 )] public float LookSensitivity { get; set; } = 1;

	TimeSince timeSinceJump = 0;

	[RequireComponent] JumpControllerAnimator Animator { get; set; }
	[Sync] public Angles TargetAngles { get; set; }

	[Property] GameObject HitEffect { get; set; }
	[Property] GameObject JumpEffect { get; set; }
	[Property] GameObject TrailEffect { get; set; }

	Vector3 LastGroundedPos { get; set; }
	TimeSince LastSave;
	int BounceCount { get; set; }
	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
			return;

		Controller.RunByDefault = true;

		Controller.WalkSpeed = WalkSpeed;
		Controller.RunSpeed = RunSpeed;
		Controller.DuckedSpeed = DuckedSpeed;

		LastGroundedPos = GameObject.WorldPosition;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		UpdateEyeAngles();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;

		if ( PlayerStats != null )
		{
			PlayerStats.TimePlayed += Time.Delta;
		}

		if ( Controller.TimeSinceGrounded < 0.2f )
		{
			TriggerLandingEvent( Controller.Velocity.z );
		}

		if ( Controller.IsOnGround )
		{
			TryJump();

			if ( LastSave > 7 )
			{
				PlayerStats?.SaveStats();
				LastSave = 0;
			}
			LastGroundedPos = GameObject.WorldPosition;
			BounceCount = 0;
		}
		else if( Controller.TimeSinceGrounded > 0.2f )
		{
			TryBounce();
		}

		// While charging jump: no movement, but input still updates & animator still runs
		if ( TimeSinceJumpDown > 0 )
		{
			Controller.WishVelocity = 0;
			Controller.Body.Velocity = 0;
			return;
		}

		InputMove();
	}

	public void TriggerLandingEvent( float velocity )
	{
		if ( GetFallDamage( Controller.Velocity.z ) >= 2 )
		{
			var fallMessage = GameObject.GetComponentInChildren<JumperFallMessage>();
			fallMessage.DisplayMessage( GetRandomFallMessage() );
			PlayerStats.TotalFalls++;
			//HasLanded = true;
		}

		if ( DistanceFell( LastGroundedPos, GameObject.WorldPosition ) > 5000 )
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
	float DistanceFell( Vector3 Start, Vector3 End )
	{
		return Math.Abs( Start.z - End.z );
	}

	void UpdateEyeAngles()
	{
		var input = Input.AnalogLook;

		input *= LookSensitivity;

		IEvents.PostToGameObject( GameObject, x => x.OnEyeAngles( ref input ) );

		var ee = Controller.EyeAngles;
		ee += input;
		ee.roll = 0;

		if ( PitchClamp > 0 )
		{
			ee.pitch = ee.pitch.Clamp( -PitchClamp, PitchClamp );
		}

		Controller.EyeAngles = ee;
	}

	void InputMove()
	{
		// Rotation from look
		var rot = Controller.EyeAngles.ToRotation();

		// Always compute WishVelocity, even in the air
		var desiredMove = Controller.Mode.UpdateMove( rot, Input.AnalogMove );

		// But only apply movement to the character on ground
		if ( Controller.IsOnGround )
		{
			Controller.WishVelocity = desiredMove;
		}
	}

	public bool CanSprint()
	{
		return true;
	}

	bool CanJump;

	public void TryBounce()
	{
		var tr = TraceBBox(WorldPosition, WorldPosition + Controller.Velocity * Time.Delta );

		if ( !tr.Hit || tr.Normal.Angle( Vector3.Up ) < 80.0f ) return;

		var bounce = -tr.Normal * Controller.Body.Velocity.Dot( tr.Normal );
		Controller.Body.Velocity = ClipVelocity( Controller.Body.Velocity, tr.Normal );
		Controller.Body.Velocity += bounce;

		var bounceAngles = Rotation.LookAt( bounce ).Angles();
		TargetAngles = bounceAngles.WithRoll( 0f );

		var effect = HitEffect.Clone();
		effect.WorldPosition = tr.EndPosition;
		effect.WorldRotation = Rotation.LookAt( tr.Normal );

		//SceneUtility.Instantiate( HitEffect, Transform.Position + Vector3.Up * 32, Rotation.LookAt( tr.Normal ) );
		Sound.Play( "jumper.impact.wall", WorldPosition );

		
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
	Vector3 Mins => new Vector3( -16, -16, 0 );
	Vector3 Maxs => new Vector3( 16, 16, 62 );
	PhysicsTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Mins, Maxs, liftFeet );
	}
	Vector3 TraceOffset;
	public PhysicsTraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
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

	[Rpc.Broadcast( NetFlags.OwnerOnly | NetFlags.Unreliable )]
	public void OnJumped()
	{
		if ( Controller.Renderer.IsValid() )
		{
			Controller.Renderer.Set( "b_jump", true );
		}
	}

	public float TimeSinceJumpDown { get; set; }
	public float TimeUntilMaxJump => 2.0f;
	float MaxJumpStrength => 885.0f;

	public void TryJump()
	{

		if ( Input.Down( "reload" ) )
		{
			TimeSinceJumpDown = 0;
			CanJump = false;
			Animator.DuckLevel = 0;
			return;
		}

		if ( Input.Down( "jump" ) && CanJump )
		{
			TimeSinceJumpDown += Time.Delta;
			var alpha = TimeSinceJumpDown.LerpInverse( 0, TimeUntilMaxJump );
			Controller.Body.Velocity = 0;
			Animator.DuckLevel = alpha;
		}

		var jumpAlpha = TimeSinceJumpDown / TimeUntilMaxJump;

		if ( jumpAlpha >= 1 || (!Input.Down( "jump" ) && jumpAlpha > 0) )
		{
			TimeSinceJumpDown = 0;

			jumpAlpha = Math.Min( 0.4f + jumpAlpha, 1.0f );
			jumpAlpha = ((int)(jumpAlpha * 10.0f)) / 10.0f;

			var vel = Vector3.Zero;

			if ( Input.AnalogMove.Length > 0.01f )
			{
				vel += Animator.Renderer.WorldRotation.Forward
					.WithZ( 0 )
					.Normal * (jumpAlpha * MaxJumpStrength * 0.5f);
			}

			// Always add vertical jump
			vel = vel.WithZ( jumpAlpha * MaxJumpStrength );

			Controller.Body.Velocity = vel;
			Controller.PreventGrounding( 0.1f );

			var effect = JumpEffect.Clone();
			effect.WorldPosition = WorldPosition;
			effect.WorldRotation = Rotation.LookAt( Controller.Velocity );
			var snd = Sound.Play( "jumper.jump", GameObject.WorldPosition );
			snd.Pitch = 1.0f - (0.5f * jumpAlpha) * 1.5f;

			PlayerStats.TotalJumps++;

			Animator?.TriggerJump();
		}

		if ( !Input.Down( "jump" ) )
		{
			Animator.DuckLevel = 0;
			TimeSinceJumpDown = 0;
			CanJump = true;
		}
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
	void GetAchievement( string cheevo )
	{
		if ( IsProxy )
			return;

		Achievements.Unlock( cheevo );

		Log.Info( $"Unlocked achievement: {cheevo}" );
	}
}
