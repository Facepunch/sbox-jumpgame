using Sandbox.Movement;

/// <summary>
/// The character is walking
/// </summary>
[Icon( "transfer_within_a_station" ), Group( "Movement" ), Title( "MoveMode - Walk (Custom)" )]
public partial class MoveModeWalk : MoveMode
{
	[Property] public int Priority { get; set; } = 0;

	[Property] public float GroundAngle { get; set; } = 45.572996f; // acos( 0.7 ) in degrees
	[Property] public float StepUpHeight { get; set; } = 18.0f;
	[Property] public float StepDownHeight { get; set; } = 18.0f;

	[RequireComponent]
	public PlayerInput PlayerInput { get; set; }

	/// <summary>Allow input-driven acceleration while airborne. Off by default, jumps are committed in this game.</summary>
	[Property, Group( "Air Movement" )] public bool EnableAirStrafing { get; set; } = false;

	[Property, Group( "Air Movement" )] public float AirAcceleration { get; set; } = 10.0f;

	/// <summary>Wish-direction speed cap; turning can still build total speed.</summary>
	[Property, Group( "Air Movement" )] public float AirSpeedCap { get; set; } = 30.0f;

	[Property, Group( "Ground Movement" )] public float GroundAcceleration { get; set; } = 10;

	/// <summary>Ground friction, scaled by BrakePower.</summary>
	[Property, Group( "Ground Movement" )] public float Friction { get; set; } = 4;

	/// <summary>Minimum speed used to calculate ground friction.</summary>
	[Property, Group( "Ground Movement" )] public float StopSpeed { get; set; } = 100;

	/// <summary>Per-axis velocity limit, not a total speed cap.</summary>
	[Property] public float MaxVelocity { get; set; } = 3500;

	public override bool AllowGrounding => true;
	public override bool AllowFalling => true;

	public override int Score( PlayerController controller ) => Priority;

	// Ported from engine Walk.cs. MovementMath is internal to the engine, so the
	// Quake-style friction/accelerate helpers are reimplemented below.
	public override void AddVelocity()
	{
		Controller.WishVelocity = Controller.WishVelocity.WithZ( 0 );
		var body = Controller.Body;
		var wish = Controller.WishVelocity;
		var wishSpeed = wish.Length;
		var direction = wishSpeed > 0 ? wish / wishSpeed : Vector3.Zero;

		if ( !Controller.IsOnGround )
		{
			if ( EnableAirStrafing )
				body.Velocity = Accelerate( body.Velocity, direction, MathF.Min( wishSpeed, MathF.Max( AirSpeedCap, 0 ) ),
					MathF.Max( AirAcceleration, 0 ) * wishSpeed );
			return;
		}

		var groundVelocity = Controller.GroundVelocity;
		var velocity = (body.Velocity - groundVelocity).WithZ( 0 );

		velocity = ApplyFriction( velocity, MathF.Max( Friction * Controller.BrakePower, 0 ) * MathF.Max( Controller.GroundFriction, 0 ) );
		velocity = Accelerate( velocity, direction, wishSpeed, MathF.Max( GroundAcceleration, 0 ) * wishSpeed );
		if ( velocity.Length < 1 ) velocity = 0;

		// Project onto the ground plane so slopes keep horizontal speed
		var normal = Controller.GroundNormal;
		if ( normal.z > 0 )
			velocity -= Vector3.Up * (velocity.Dot( normal ) / normal.z);

		velocity += groundVelocity;
		body.Velocity = velocity.Clamp( -MaxVelocity, MaxVelocity );
	}

	Vector3 ApplyFriction( Vector3 velocity, float friction )
	{
		var speed = velocity.Length;
		if ( speed < 0.1f ) return 0;

		var control = MathF.Max( speed, MathF.Max( StopSpeed, 0 ) );
		var newSpeed = MathF.Max( speed - control * friction * Time.Delta, 0 );
		return velocity * (newSpeed / speed);
	}

	static Vector3 Accelerate( Vector3 velocity, Vector3 direction, float wishSpeed, float accel )
	{
		var addSpeed = wishSpeed - velocity.Dot( direction );
		if ( addSpeed <= 0 ) return velocity;

		return velocity + direction * MathF.Min( accel * Time.Delta, addSpeed );
	}

	public override void PrePhysicsStep()
	{
		base.PrePhysicsStep();

		// Apply air drag once before the solver so contact correction cannot undo it
		if ( !Controller.IsOnGround )
			Controller.Body.Velocity /= 1 + MathF.Max( Controller.AirFriction, 0 ) * Time.Delta;

		if ( Controller.IsOnGround && StepUpHeight > 0 )
		{
			TrySteppingUp( StepUpHeight );
		}
	}

	public override void PostPhysicsStep()
	{
		base.PostPhysicsStep();

		StickToGround( StepDownHeight );
	}

	public override bool IsStandableSurface( in SceneTraceResult result )
	{
		if ( Vector3.GetAngle( Vector3.Up, result.Normal ) > GroundAngle )
			return false;

		return true;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		// ignore pitch when walking
		eyes = eyes.Angles() with { pitch = 0 };

		input = input.ClampLength( 1f );
		Vector3 vector = eyes * input;
		bool flag = PlayerInput.CanSprint() && Input.Down( Controller.AltMoveButton );
		if ( Controller.RunByDefault )
		{
			flag = !flag;
		}

		float num = (flag ? Controller.RunSpeed : Controller.WalkSpeed);
		if ( Controller.IsDucking )
		{
			num = Controller.DuckedSpeed;
		}

		if ( vector.IsNearlyZero( 0.1f ) )
			return 0f;

		// No input smoothing, matching upstream: ground acceleration/friction in AddVelocity handle ramping
		return vector * num;
	}

	public override Transform CalculateEyeTransform()
	{
		var tr = base.CalculateEyeTransform();

		//
		// Hack central, but this just moves the eye position a bit so it works better with hand IK
		//
		tr = tr with { Position = tr.Position - Vector3.Up * 0f + tr.Forward * -5f };

		//
		// :(
		//
		if ( Controller.IsDucking )
		{
			tr = tr with { Position = tr.Position + Vector3.Up * 20f };
		}

		return tr;
	}
}
