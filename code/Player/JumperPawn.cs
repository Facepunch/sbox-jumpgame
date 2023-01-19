using Sandbox;

internal partial class JumperPawn : Sandbox.Player
{

	[Net]
	public AnimatedEntity Citizen { get; set; }

	[Net] public Entity LookTarget { get; set; }

	[Net]
	public bool TalkingToNPC { get; set; }

	[Net]
	public Entity NPCCameraTarget { get; set; }

	[Net]
	public Vector3 NPCCamera { get; set; }

	public const float MaxRenderDistanceOther = 256f;
	public const float MaxRenderDistanceSelf = 256f;

	[Net, Predicted]
	public float Height { get; set; }
	[Net, Predicted]
	public float MaxHeight { get; set; }
	public int TotalJumps { get; set; }
	public int TotalFalls { get; set; }
	public int Completions { get; set; }

	[Net, Predicted]
	public float BestHeight { get; set; }

	[Net]
	public PropCarriable HeldBody { get; set; }

	[Net]
	public string rndColor { get; set; } = "#eb4034";

	public Particles falleffect { get; private set; }

	private JumperAnimator Animator;
	private JumperCamera JumperCamera = new();

	public override void Respawn()
	{
		base.Respawn();

		SetModel( "models/citizen/citizen.vmdl" );

		Citizen = this;

		Controller = new JumperController();

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

		rndColor = $"{Color.Random.Hex}";

		Tags.Add( "JumpPlayer" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var progress = Progress.Current;

		TotalFalls = progress.TotalFalls;
		TotalJumps = progress.TotalJumps;

		Completions = progress.NumberCompletions;

		if ( MaxHeight < progress.BestHeight )
		{
			MaxHeight = progress.BestHeight;
			BestHeight = progress.BestHeight;
		}

		if ( progress.TimePlayed == 0 ) return;

		if ( !Game.IsToolsEnabled )
		{
			SetPosition( progress.Position, progress.Angles );
		}
	}

	[Event.Tick.Server]
	public void UpdateBestHeight()
	{
		if ( Game.IsClient )
		{
			BestHeight = Progress.Current.BestHeight;
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		//CameraMode = new RagdollCamera();
	}
	public float TimePlayed;
	private TimeSince TimeSinceProgressSaved = 0f;
	private TimeSince TimeSinceSubmitSaved = 0f;
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( TalkingToNPC )
			return;

		Animator ??= new( this );
		Animator.Simulate();

		if ( Game.IsServer && TimeSinceSubmitSaved > 10f && !Game.IsToolsEnabled )
		{
			TimeSinceSubmitSaved = 0f;
			//JumperGame.SubmitScore(Client, (int)MaxHeight );
			//JumperGame.SubmitScore(Client, (int)MaxHeight );
		}
		
		Height = MathX.CeilToInt( Position.z - JumperGame.Current.StartHeight );
		MaxHeight = Math.Max( Height, MaxHeight );

		if ( !Game.IsClient ) return;

		var progress = Progress.Current;
		
		if ( !Game.IsToolsEnabled )
		{
			if( progress.BestHeight < MaxHeight)
			{
				progress.BestHeight = MaxHeight;

			}

			progress.TimePlayed += Time.Delta;

			progress.TotalFalls = TotalFalls;
			progress.TotalJumps = TotalJumps;

			TimePlayed = progress.TimePlayed;
		}

		if ( LookTarget.IsValid() )
		{
			if ( Animator is JumperAnimator animator )
			{
				//animator.LookAtMe = true;

				SetAnimLookAt( "aim_eyes", EyePosition, LookTarget.Position + Vector3.Up  );
				SetAnimLookAt( "aim_head", EyePosition, LookTarget.Position + Vector3.Up  );
				SetAnimLookAt( "aim_body", EyePosition, LookTarget.Position + Vector3.Up  );
			}
			//CameraMode = new LookAtCamera();

			//if ( CameraMode is LookAtCamera lookAtCamera )
			//{
			//	lookAtCamera.TargetEntity = NPCCameraTarget;
			//	lookAtCamera.TargetOffset = new Vector3( 0, 0, 64 );
			//	lookAtCamera.FieldOfView = 70;
			//	lookAtCamera.MaxFov = 70;
			//	lookAtCamera.MinFov = 50;
			//	lookAtCamera.Origin = NPCCamera;
			//}
		}

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
		
		if ( GroundEntity.IsValid() && !Game.IsToolsEnabled )
		{
			progress.Position = Position;
			progress.Angles = Rotation.Angles();
		}

		if( Game.IsClient && TimeSinceProgressSaved > 5f )
		{
			TimeSinceProgressSaved = 0f;
			progress.Save();

		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( !cl.IsOwnedByLocalClient ) return;

		JumperCamera?.Update();
	}

	[Event.Client.Frame]
	private void UpdateRenderAlpha()
	{

		var dist = Camera.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceSelf, MaxRenderDistanceSelf * .1f );
		a = Math.Max( a, .15f );
		a = Sandbox.Utility.Easing.EaseOut( a );

		RenderColor = RenderColor.WithAlpha( a );

		foreach ( var child in Children )
		{
			if ( child is not ModelEntity m || !child.IsValid() ) continue;
			m.RenderColor = m.RenderColor.WithAlpha( a );
		}
	}

	[Event.Client.Frame]
	private void UpdateRenderAlphaOthers()
	{
		if ( !Game.LocalPawn.IsValid() || Game.LocalPawn == this )
			return;

		var dist = Game.LocalPawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistanceOther, MaxRenderDistanceOther * .1f );
		a = Math.Max( a, .15f );
		a = Sandbox.Utility.Easing.EaseOut( a );

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

		if ( !Game.IsServer )
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
