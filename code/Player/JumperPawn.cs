
internal partial class JumperPawn : Sandbox.Player
{

	public const float MaxRenderDistance = 128f;

	[Net]
	public float Height { get; set; }
	[Net]
	public float MaxHeight { get; set; }
	[Net, Predicted]
	public int TotalJumps { get; set; }
	[Net, Predicted]
	public int TotalFalls { get; set; }

	[Net]
	public PropCarriable HeldBody { get; set; }

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new JumperController();
		Animator = new JumperAnimator();
		CameraMode = new JumperCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if ( Client != null )
		{
			var clothing = new ClothingContainer();
			clothing.LoadFromClient( Client );
			clothing.DressEntity( this );
		}

		Tags.Add( "JumpPlayer" );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new RagdollCamera();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsServer )
		{
			Height = MathX.CeilToInt( Position.z );
			MaxHeight = Math.Max( Height, MaxHeight );
		}
	}

	[Event.Frame]
	private void UpdateRenderAlpha()
	{
		if ( !Local.Pawn.IsValid() || Local.Pawn == this )
			return;

		var dist = Local.Pawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
		a = Math.Max( a, .15f );
		a = Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}

	TimeSince timeSinceLastFootstep = 0;
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsServer )
			return;

		if ( foot == 0 )
		{
			//var lfoot = Particles.Create( "particles/gameplay/player/footsteps/footstep_l.vpcf", pos );
			//lfoot.SetOrientation( 0, Transform.Rotation );
		}
		else
		{
			//var rfoot = Particles.Create( "particles/gameplay/player/footsteps/footstep_r.vpcf", pos );
			//rfoot.SetOrientation( 0, Transform.Rotation );
		}

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

}
