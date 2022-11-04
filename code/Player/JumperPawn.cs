
internal partial class JumperPawn : Sandbox.Player
{

	public const float MaxRenderDistanceOther = 256f;
	public const float MaxRenderDistanceSelf = 256f;

	[Net, Predicted]
	public float Height { get; set; }
	[Net, Predicted]
	public float MaxHeight { get; set; }
	[Net, Predicted]
	public int TotalJumps { get; set; }
	[Net, Predicted]
	public int TotalFalls { get; set; }

	[Net]
	public PropCarriable HeldBody { get; set; }

	public Particles falleffect { get; private set; }

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

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var progress = Progress.Current;
		if ( progress.TimePlayed == 0 ) return;

		SetPosition( progress.Position, progress.Angles );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new RagdollCamera();
	}
	public float TimePlayed;
	private TimeSince TimeSinceProgressSaved = 0f;
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Height = MathX.CeilToInt( Position.z - JumperGame.Current.StartHeight );
		MaxHeight = Math.Max( Height, MaxHeight );

		if ( !IsClient ) return;

		var progress = Progress.Current;

		progress.BestHeight = MaxHeight;
		progress.TimePlayed += Time.Delta;

		TimePlayed = progress.TimePlayed;

		if ( falleffect == null)
		{
			falleffect = Particles.Create( "particles/player/falling/jumper.falling.vpcf", this );
		}

		if ( Velocity.z < -700 )
		{
			falleffect.SetPosition( 1, new Vector3(0,0, Velocity.z) );

		}
		else
		{
			falleffect.SetPosition( 1, new Vector3( 0, 0, 0 ) );
		}
		
		if ( GroundEntity.IsValid() )
		{
			progress.Position = Position;
			progress.Angles = Rotation.Angles();
		}

		if( IsClient && TimeSinceProgressSaved > 5f )
		{
			TimeSinceProgressSaved = 0f;
			progress.Save();
		}
	}

	[Event.Frame]
	private void UpdateRenderAlpha()
	{

		var dist = CameraMode.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceSelf, MaxRenderDistanceSelf * .1f );
		a = Math.Max( a, .15f );
		a = Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}

	[Event.Frame]
	private void UpdateRenderAlphaOthers()
	{
		if ( !Local.Pawn.IsValid() || Local.Pawn == this )
			return;

		var dist = Local.Pawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceOther, MaxRenderDistanceOther * .1f );
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

	[ConCmd.Server]
	public static void SetPosition( Vector3 position, Angles angles )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller?.Pawn is not JumperPawn p ) return;

		p.Position = position + Vector3.Up;
		p.Rotation = Rotation.From( angles );
	}

}
