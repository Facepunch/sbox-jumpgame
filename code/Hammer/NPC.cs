[GameResource( "NPC TEXT", "npct", "Npc_text", Icon = "sentiment_very_satisfied", IconBgColor = "#fcba03", IconFgColor = "#363324" )]
public class NPCTextGameResource : GameResource
{
	[Property]
	[Title("Name")]
	[Category( "Name" )]
	public String NPCName { get; set; }
	
	[Property]
	[Title( "Text" )]
	[Category( "Text" )]
	public List<String> NPCText { get; set; } = new();

	[Property, ResourceType("sound")]
	[Title( "Voice" )]
	[Category( "Voice" )]
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

	[Property]
	[MinMax( 0.8f, 1.2f )]
	public float NpcSize { get; set; } = 1f;

	[Net]
	protected NPCTextGameResource Resource { get; set; }

	[Net] 
	public Entity LookTarget { get; set; }
	public JumperAnimator Animator { get; private set; }
	public Vector3 WishVelocity { get; set; }

	public override void Spawn()
	{
		Animator = new JumperAnimator();
		SetModel( "models/frogfella/frog_test_subject_01a.vmdl" );

		Resource = ResourceLibrary.Get<NPCTextGameResource>( AssetPath );
		System.Random rnd = new System.Random();
		if ( rnd.Next(2) == 0 )
		{
			SetMaterialGroup( "ORANGE" );
		}
		
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

	[Event.Entity.PostSpawn]
	public void PostSpawn()
	{
		SetAnimParameter( "scale_height", NpcSize );
	}
	
	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( !IsServer ) return;
		
		if ( other is not JumperPawn pl ) return;

		LookTarget = pl;
		LookAtPlayer( LookTarget );
		pl.NPCCameraTarget = this;
		pl.LookTarget = this;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsServer ) return;

		if ( other is not JumperPawn pl ) return;
	
		TalkToPlayer(To.Single(pl.Client), GetRandomMessage() );
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
				
				Rotation = Rotation.Slerp( Rotation.Angles().WithRoll( 0f ).WithPitch( 0f ).ToRotation(), Rotation.From( defaultPosition ), Time.Delta * .5f);

				SetAnimParameter( "b_shuffle", Rotation != Rotation.From( defaultPosition ) );
			}
		}
	}

	private int lastFallMessage;
	private string GetRandomMessage()
	{
		var idx = Rand.Int( 0, Resource.NPCText.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Rand.Int( 0, Resource.NPCText.Count - 1 );

		lastFallMessage = idx;
		return string.Format( Resource.NPCText[idx] );
	}
}
