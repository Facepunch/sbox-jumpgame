using Sandbox.Movement;

/// <summary>
/// The character is walking
/// </summary>
[Icon( "transfer_within_a_station" ), Group( "Movement" ), Title( "MoveMode - Walk (Custom)" )]
public partial class MoveModeWalk : MoveMode
{
	[Property] public int Priority { get; set; } = 0;

	[Property] public float GroundAngle { get; set; } = 45.0f;
	[Property] public float StepUpHeight { get; set; } = 18.0f;
	[Property] public float StepDownHeight { get; set; } = 18.0f;

	[RequireComponent]
	public PlayerInput PlayerInput { get; set; }

	Vector3.SmoothDamped smoothedMovement;

	public override bool AllowGrounding => true;
	public override bool AllowFalling => true;

	public override int Score( PlayerController controller ) => Priority;

	public override void AddVelocity()
	{
		Controller.WishVelocity = Controller.WishVelocity.WithZ( 0 );
		base.AddVelocity();
	}

	public override void PrePhysicsStep()
	{
		base.PrePhysicsStep();

		if ( StepUpHeight > 0 )
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
		{
			vector = 0f;
		}
		else
		{
			smoothedMovement.Current = vector.Normal * smoothedMovement.Current.Length;
		}

		smoothedMovement.Target = vector * num;
		smoothedMovement.SmoothTime = ((smoothedMovement.Target.Length < smoothedMovement.Current.Length) ? Controller.DeaccelerationTime : Controller.AccelerationTime);
		smoothedMovement.Update( Time.Delta );
		if ( smoothedMovement.Current.IsNearlyZero( 0.01f ) )
		{
			smoothedMovement.Current = 0f;
		}

		return smoothedMovement.Current;
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
