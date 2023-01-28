[Library( "js_wind" )]
[AutoApplyMaterial( "materials/dev/wind/dev_wind.vmat" )]
[Display( Name = "Wind", GroupName = "Jumper", Description = "Wind" ), Category( "Triggers" ), Icon( "landscape" )]
[HammerEntity]
[DrawAngles( nameof( WindDirection) )]
public partial class Wind : BaseTrigger
{
	/// <summary>
	/// It will go the direction set then it will go the opposite direction.
	/// </summary>
	[Net, Property( Title = "Wind Direction" )]
	public Angles WindDirection { get; set; }

	[Net] public string ParticleSystemName { get; set; } = "particles/gameplay/wind/wind_dir.vpcf";
	private Particles ActiveSystem;

	[Net]public Vector3 WindDirectional { get; set; }
	[Net]public Vector3 ParticleWindDirectional { get; set; }
	
	public TimeSince TimeSinceLastWind = 0f;
	public bool WindToggle = false;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "trigger" );

		Transmit = TransmitType.Always;

		EnableDrawing = false;

		EnableTouchPersists = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static , false );

		EnableAllCollisions = false;
		EnableTouch = true;
	}
	
	[Event.Tick.Client]
	void ClientTick()
	{
		if ( ActiveSystem != null )
		{
			ActiveSystem.SetPosition( 0, Game.LocalPawn.Position + WindDirectional.Normal * -200 );
			ActiveSystem.SetForward( 0, WindDirectional * 10f );
		}
	}


	[Event.Tick.Server]
	public void WindCheck()
	{
		if ( TimeSinceLastWind > 6.5f )
		{
			TimeSinceLastWind = 0f;
			WindToggle = !WindToggle;
		}
		if ( WindToggle )
		{
			WindDirectional = WindDirection.Forward;
		}
		else
		{
			WindDirectional = WindDirection.Forward * -1;

		}		
		//DebugOverlay.Line( Position, Position + WindDirectional.Normal * 300f, .1f, false );
		//DebugOverlay.Text( $"Time Since Last Wind: {TimeSinceLastWind}", Position + Vector3.Up * 20f );
		//DebugOverlay.Axis( Position + WindDirection.Forward, Rotation, 10f );
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !Game.IsClient ) return;
		if ( other is not JumperPawn pawn ) return;
		if ( !pawn.IsLocalPawn ) return;

		ActiveSystem = Particles.Create( ParticleSystemName );
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( other is not JumperPawn pawn ) return;

		if ( !pawn.GroundEntity.IsValid() )
		{
			pawn.Velocity += WindDirectional * 15;		
		}
		else
		{
			pawn.Velocity += WindDirectional * 25;
			pawn.TouchingMoveable = true;
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not JumperPawn pawn ) return;

		pawn.TouchingMoveable = false;
		ActiveSystem?.Destroy();
	}
}
