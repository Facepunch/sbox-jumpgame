
using Sandbox;
using Sandbox.Internal;
using System.Numerics;

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

	[Net]
	public bool IsEditorMode { get; set; }

	private bool _hasCheated;

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
		else
		{
			IsEditorMode = Host.IsToolsEnabled;
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

		if(IsEditorMode)
		{
			Log.Info( "You are in editor mode, you can't save progress" );
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
	public override void DoPlayerNoclip( Client player )
	{
		// Do nothing. The player can't noclip in this mode.

		if(IsEditorMode)
		{
			if ( !player.HasPermission( "noclip" ) )
				return;

			if ( player.Pawn is Player basePlayer )
			{
				if ( basePlayer.DevController is NoclipController )
				{
					Log.Info( "Noclip Mode Off" );
					basePlayer.DevController = null;
				}
				else
				{
					Log.Info( "Noclip Mode On" );
					basePlayer.DevController = new NoclipController();
				}
			}
		}
	}

	public override void DoPlayerSuicide( Client client )
	{
		// Do nothing. The player can't suicide in this mode.
	}

	[ConCmd.Admin]
	public static void ResetPlayer( )
	{
		if ( ConsoleSystem.Caller.Pawn is JumperPawn player )
		{
			var spawnpoints = All.OfType<SpawnPoint>();
			var randomSpawnPoint = spawnpoints.OrderBy( x => Rand.Int( 999 ) ).FirstOrDefault();

			if ( randomSpawnPoint != null )
			{
				var tx = randomSpawnPoint.Transform;
				tx.Position += Vector3.Up * 50.0f;
				player.Transform = tx;
			}
		}
	}

	[ConCmd.Server]
	public static void SendChat( string message )
	{
		var caller = ConsoleSystem.Caller;

		ReceiveChat( To.Everyone, caller.Name, message );
	}

	[ConCmd.Server]
	public static void Submit( Client cl, float score)
	{
		SubmitScore( cl, (int)score );
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

	private static string GetMapBucket()
	{
		var map = Global.MapName;

		return $"Map-{map}-Current-Height";
	}
	
	private bool CanSubmitScore( JumperPawn player )
	{
		return !Host.IsToolsEnabled && player.Client != null;
	}
	public static async void SubmitScore( Client client, int score )
	{
		var leaderboard = await Leaderboard.FindOrCreate( GetMapBucket(), false );

		await leaderboard.Value.Submit( client, score );

	}
}
