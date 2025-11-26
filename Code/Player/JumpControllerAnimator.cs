using static Sandbox.PhysicsContact;

namespace Sandbox;

public sealed class JumpControllerAnimator : Component
{
	SkinnedModelRenderer _renderer;
	/// <summary>
	/// The body will usually be a child object with SkinnedModelRenderer
	/// </summary>
	[Property, Feature( "Animator" )]
	public SkinnedModelRenderer Renderer
	{
		get => _renderer;
		set
		{
			if ( _renderer == value ) return;

			_renderer = value;
		}
	}
	[Property, RequireComponent] PlayerController Controller { get; set; }

	[Sync] public Angles TargetAngles { get; set; }

	protected override void OnUpdate()
	{
		WithVelocity( Controller.Velocity );
		WithWishVelocity( Controller.WishVelocity );
		IsGrounded = Controller.IsOnGround;

		Vector3 dir = default;

		// 1) If we have move input, use that to define facing
		var moveInput = Input.AnalogMove;
		if ( moveInput.Length > 0.01f )
		{
			// Convert 2D move into world-space using eye yaw (pitch ignored)
			var eye = Controller.EyeAngles;
			eye.pitch = 0;
			eye.roll = 0;

			dir = (eye.ToRotation() * moveInput).WithZ( 0 );
		}
		// 2) Otherwise, fall back to velocity (e.g. sliding / being pushed)
		else if ( Controller.WishVelocity.Length > 0.01f )
		{
			dir = Controller.WishVelocity.WithZ( 0 );
		}

		if ( !dir.IsNearlyZero( 0.001f ) && Controller.IsOnGround )
		{
			TargetAngles = Rotation
				.LookAt( dir.Normal )
				.Angles()
				.WithRoll( 0 );
		}

		if ( !Controller.IsOnGround )
		{
			TargetAngles = Rotation.LookAt( Controller.Velocity.WithZ( 0 ).Normal ).Angles().WithRoll( 0 );
		}

		Renderer.WorldRotation = Rotation.Slerp(
			Renderer.WorldRotation,
			Rotation.From( TargetAngles ),
			8f * Time.Delta
		);

		WithLook( Controller.EyeAngles.Forward, 1, 1, 1.0f );
	}

	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Renderer.WorldRotation.Forward.Dot( dir );
		var sideward = Renderer.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Renderer.Set( "move_direction", angle );
		Renderer.Set( "move_speed", Velocity.Length );
		Renderer.Set( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		Renderer.Set( "move_y", sideward );
		Renderer.Set( "move_x", forward );
		Renderer.Set( "move_z", Velocity.z );
	}

	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = Renderer.WorldRotation.Forward.Dot( dir );
		var sideward = Renderer.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		Renderer.Set( "wish_direction", angle );
		Renderer.Set( "wish_speed", Velocity.Length );
		Renderer.Set( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		Renderer.Set( "wish_y", sideward );
		Renderer.Set( "wish_x", forward );
		Renderer.Set( "wish_z", Velocity.z );
	}

	public bool IsGrounded
	{
		get => Renderer.GetBool( "b_grounded" );
		set => Renderer.Set( "b_grounded", value );
	}

	public void WithLook( Vector3 lookDirection, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
	{
		Renderer.SetLookDirection( "aim_eyes", lookDirection, eyesWeight );
		Renderer.SetLookDirection( "aim_head", lookDirection, headWeight );
		Renderer.SetLookDirection( "aim_body", lookDirection, bodyWeight );
	}

	public float DuckLevel
	{
		get => Renderer.GetFloat( "duck" );
		set => Renderer.Set( "duck", value );
	}
	public void TriggerJump()
	{
		Renderer.Set( "b_jump", true );
	}
}
