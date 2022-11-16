
public partial class JumperGame : Game
{

	public new static JumperGame Current;

	[Net]
	public RealTimeSince SessionTimer { get; set; } = 0f;
	[Net]
	public int EndHeight { get; set; }
	[Net]
	public int StartHeight { get; set; }

	[Net]
	public int MapLength { get; set; }

	public JumperGame()
	{
		Current = this;
		if ( IsServer)
		{
			AddToPrecache();
		}
		
		if ( IsClient )
		{
			new JumperRootPanel();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient )
		{
			Progress.Current.Save();
		}
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		var endpoint = Entity.All.OfType<EndPoint>().FirstOrDefault();
		if( endpoint != null )
		{
			EndHeight = (int)endpoint.Position.z;
		}

		var startpoint = Entity.All.OfType<StartPoint>().FirstOrDefault();
		if ( startpoint != null )
		{
			StartHeight = (int)startpoint.Position.z;
		}
		var distanceBetween = Vector3.DistanceBetween( new Vector3(0,0, StartHeight ), new Vector3( 0, 0, EndHeight) );
		MapLength = (int)distanceBetween;
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		client.Pawn = new JumperPawn();
		(client.Pawn as JumperPawn).Respawn();

		var spawnpoints = All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Rand.Int( 999 ) ).FirstOrDefault();

		ReceiveChat( To.Everyone, client.Name, " has joined the game" );

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position += Vector3.Up * 50.0f;
			client.Pawn.Transform = tx;
		}
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		ReceiveChat( To.Everyone, cl.Name, reason.ToString() );
	}

	[ConCmd.Server]
	public static void SendChat( string message )
	{
		var caller = ConsoleSystem.Caller;

		ReceiveChat( To.Everyone, caller.Name, message );
	}

	[ConCmd.Client( "receive_chat", CanBeCalledFromServer = true )]
	public static void ReceiveChat( string name, string message )
	{
		Event.Run( "chat.received", name, message );
	}

	[ConCmd.Server]
	public static void SendTease( string message)
	{
		var caller = ConsoleSystem.Caller;

		TeasePlayer( To.Single( caller ), message );
	}

	[ConCmd.Client( "tease_player", CanBeCalledFromServer = true )]
	public static void TeasePlayer( string message )
	{
		NPCTalk.Display( message);
	}

	[ConCmd.Server]
	public static void NPCTalking( string message, string Voice, string npcname )
	{
		var caller = ConsoleSystem.Caller;

		NPCTalker( To.Single( caller ), message, Voice, npcname );
	}

	[ConCmd.Client( "npc_talker", CanBeCalledFromServer = true )]
	public static void NPCTalker( string message, String Voice, string npcname )
	{
		NPCCharacter.Display( message, Voice, npcname );
	}
}
