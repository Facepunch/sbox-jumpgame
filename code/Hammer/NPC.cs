[GameResource( "NPC TEXT", "npct", "Npc_text" )]
public class NPCTextGameResource : GameResource
{
	[Property]
	public String NPCName { get; set; }
	
	[Property]
	public List<String> NPCText { get; set; } = new();

	[Property, ResourceType("sound")]
	public string NPCVoice { get; set; }
}

[Model( Model = "models/citizen/citizen.vmdl" )]
[Library( "js_npc_text" )]
[Display( Name = "NPX Text", GroupName = "Jumper", Description = "A NPC that talks to you." ), Category( "Gameplay" ), Icon( "sports_gymnastics" )]
[BoundsHelper( "mins", "maxs", true, false )]
[HammerEntity]
public partial class NPC : AnimatedEntity
{
	[Property( "mins", Title = "Checkpoint mins" )]
	[DefaultValue( "-75 -75 0" )]
	[Net]
	public Vector3 Mins { get; set; } = new Vector3( -75, -75, 0 );

	[Property( "maxs", Title = "Checkpoint maxs" )]
	[DefaultValue( "75 75 100" )]
	[Net]
	public Vector3 Maxs { get; set; } = new Vector3( 75, 75, 100);

	[Property, ResourceType( "npct" )]
	public string AssetPath { get; set; }

	[Net]
	protected NPCTextGameResource Resource { get; set; }

	[Net, Property, FGDType( "target_destination" )] public string TargetEntity { get; set; } = "";
	[Net, Property, FGDType( "target_destination" )] public string PLTargetEntity { get; set; } = "";

	[Net] 
	public Entity LookTarget { get; set; }
	public JumperAnimator Animator { get; private set; }
	public Vector3 WishVelocity { get; set; }

	public override void Spawn()
	{
		Animator = new JumperAnimator();
		SetModel("models/citizen/citizen.vmdl");

		Resource = ResourceLibrary.Get<NPCTextGameResource>( AssetPath );

		Log.Info( GetRandomFallMessage() );

		EnableTouch = true;

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		EnableTouch = true;

		var trigger = new BaseTrigger();
		trigger.SetParent( this, null, Transform.Zero );
		trigger.SetupPhysicsFromOBB( PhysicsMotionType.Static, Mins, Maxs );
		trigger.Transmit = TransmitType.Always;
		trigger.EnableTouchPersists = true;

		base.Spawn();
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( !IsServer ) return;
		
		if ( other is not JumperPawn pl ) return;

		//var target = FindByName( TargetEntity );
		//var pltarget = FindByName( PLTargetEntity );
		//pl.Position = pltarget.Position;

		LookTarget = pl;
		LookAtPlayer( LookTarget );
		pl.NPCCameraTarget = this;

		//if ( target != null )
		//{
		//	pl.NPCCamera = target.Position;
		//}
		pl.LookTarget = this;
		//Freeze(pl);
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		TalkToPlayer( GetRandomFallMessage() );
	}

	[ClientRpc]
	public void TalkToPlayer(string msg)
	{
		JumperGame.NPCTalking( msg, Resource.NPCVoice, Resource.NPCName );
	}
	
	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not JumperPawn pl ) return;
		LookTarget = null;
		pl.LookTarget = null;
		//pl.CameraMode = new JumperCamera();
	}
	public void LookAtPlayer( Entity pl )
	{
		if ( LookTarget.IsValid() )
		{
			if ( Animator is JumperAnimator animator )
			{
				animator.LookAtMe = true;

				WishVelocity = Velocity;

				SetAnimLookAt( "aim_eyes", pl.Position + Vector3.Up * 2f );
				SetAnimLookAt( "aim_head", pl.Position + Vector3.Up * 2f );
				SetAnimLookAt( "aim_body", pl.Position + Vector3.Up * 2f );

				var defaultPosition = Rotation.LookAt( pl.Position - Position ).Angles();
				
				Rotation = Rotation.Slerp( Rotation, Rotation.From( defaultPosition ), Time.Delta * .5f);

				SetAnimParameter( "b_shuffle", Rotation != Rotation.From( defaultPosition ) );
			}
		}
	}
	//public void Freeze( JumperPawn pl )
	//{
	//}

	private int lastFallMessage;
	private string GetRandomFallMessage()
	{
		var idx = Rand.Int( 0, Resource.NPCText.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Rand.Int( 0, Resource.NPCText.Count - 1 );

		lastFallMessage = idx;
		return string.Format( Resource.NPCText[idx] );
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
		"It's a new day!"

	};
}
