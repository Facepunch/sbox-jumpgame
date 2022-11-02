
public partial class JumperGame : Game
{
	public new static JumperGame Current;

	[Net]
	public RealTimeSince SessionTimer { get; set; } = 0f;

	public JumperGame()
	{
		Current = this;
		
		if ( IsClient )
		{
			new JumperRootPanel();
		}
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
}
