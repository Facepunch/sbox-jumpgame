﻿using Sandbox;
using Sandbox.Component;

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
	public bool ReachedEnd { get; set; }

	[Net, Predicted]
	public float BestHeight { get; set; }

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
	Glow glow;
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var progress = Progress.Current;

		TotalFalls = progress.TotalFalls;
		TotalJumps = progress.TotalJumps;

		Completions = progress.NumberCompletions;

		ReachedEnd = progress.HasCompleted;

		if ( MaxHeight < progress.BestHeight )
		{
			MaxHeight = progress.BestHeight;
			BestHeight = progress.BestHeight;
		}

		if ( progress.TimePlayed == 0 ) return;

		if ( !Game.IsEditor )
		{
			SetPosition( progress.Position, progress.Angles );
		}

		glow = Components.GetOrCreate<Glow>();

		glow.Enabled = true;
		glow.Width = 0.25f;
		glow.Color = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
		glow.ObscuredColor = Color.Transparent;
		glow.InsideObscuredColor = Color.Black.WithAlpha( 0.2f );

		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			glow = child.Components.GetOrCreate<Glow>();
			glow.Enabled = true;
			glow.Color = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
			glow.ObscuredColor = Color.Transparent;
			glow.InsideObscuredColor = Color.Black.WithAlpha( 0.2f );
		}
	}
	
	public void ResetLast()
	{
		var progress = Progress.Current;
		SetPosition( progress.Position, progress.Angles );
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

		if ( Game.IsServer && TimeSinceSubmitSaved > 10f && !Game.IsEditor )
		{
			TimeSinceSubmitSaved = 0f;
			//JumperGame.SubmitScore(Client, (int)MaxHeight );
			//JumperGame.SubmitScore(Client, (int)MaxHeight );
		}
		
		Height = MathX.CeilToInt( Position.z - JumperGame.Current.StartHeight );
		MaxHeight = Math.Max( Height, MaxHeight );

		if ( !Game.IsClient ) return;

		var progress = Progress.Current;
		
		if ( !Game.IsEditor )
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
				SetAnimLookAt( "aim_eyes", EyePosition, LookTarget.Position + Vector3.Up  );
				SetAnimLookAt( "aim_head", EyePosition, LookTarget.Position + Vector3.Up  );
				SetAnimLookAt( "aim_body", EyePosition, LookTarget.Position + Vector3.Up  );
			}
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
		
		if ( GroundEntity.IsValid() && !Game.IsEditor )
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
	public void ResetStats()
	{
		if ( !Game.IsClient ) return;


		Log.Info( "Resetting stats" );
		
		var progress = Progress.Current;
		progress.HasCompleted = false;
		ReachedEnd = false;
		progress.NumberCompletions = progress.NumberCompletions + 1;
	}
	public void AtEnding()
	{
		if ( Game.IsEditor ) return;

		ReachedEnd = true;
		Completions++;
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( !cl.IsOwnedByLocalClient ) return;

		JumperCamera?.Update();
	}
}
