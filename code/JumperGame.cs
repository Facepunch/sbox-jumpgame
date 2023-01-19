
using Sandbox;
using Sandbox.Internal;
using System.Numerics;
using static Sandbox.Event;

public partial class JumperGame : GameManager
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
		if ( Game.IsServer )
		{
			AddToPrecache();
		}
		
		if ( Game.IsClient )
		{
			Game.RootPanel = new JumperRootPanel();
		}
		else
		{
			IsEditorMode = Game.IsToolsEnabled;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient )
		{
			Progress.Current.Save();
		}
	}
	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		var endpoint = Sandbox.Entity.All.OfType<EndPoint>().FirstOrDefault();
		if( endpoint != null )
		{
			EndHeight = (int)endpoint.Position.z;
		}

		var startpoint = Sandbox.Entity.All.OfType<StartPoint>().FirstOrDefault();
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

	public override void OnVoicePlayed( IClient cl )
	{
		VoiceChatList.Current?.OnVoicePlayed( cl.SteamId, cl.Voice.CurrentLevel );
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		client.Pawn = new JumperPawn();
		(client.Pawn as JumperPawn).Respawn();

		var spawnpoints = All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Game.Random.Int( 999 ) ).FirstOrDefault();

		ReceiveChat( To.Everyone, client.Name, " has joined the game" );

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position += Vector3.Up * 50.0f;
			client.Pawn.Position = tx.Position;
			client.Pawn.Rotation = tx.Rotation;
		}
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		ReceiveChat( To.Everyone, cl.Name, reason.ToString() );
	}
	public override void DoPlayerDevCam( IClient client )
	{
		// do nothing
	}

	[ConCmd.Admin]
	public static void ResetPlayer( )
	{
		if ( ConsoleSystem.Caller.Pawn is JumperPawn player )
		{
			var spawnpoints = All.OfType<SpawnPoint>();
			var randomSpawnPoint = spawnpoints.OrderBy( x => Game.Random.Int( 999 ) ).FirstOrDefault();

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

	//[ConCmd.Server]
	//public static void Submit( IClient cl, float score)
	//{
	//	SubmitScore( cl, (int)score );
	//}

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

	[ConCmd.Server]
	public static void SetPlayerPosition( Vector3 position )
	{
		if ( !(ConsoleSystem.Caller?.Pawn.IsValid() ?? false) ) 
			return;

		ConsoleSystem.Caller.Pawn.Position = position;
	}

	//private static string GetMapBucket()
	//{
	//	var map = Game.MapName;

	//	return $"Map-{map}-Current-Height";
	//}

	//private bool CanSubmitScore( JumperPawn player )
	//{
	//	return !Host.IsToolsEnabled && player.Client != null;
	//}
	//public static async void SubmitScore( IClient client, int score )
	//{
	//	var leaderboard = await Leaderboard.FindOrCreate( GetMapBucket(), false );

	//	await leaderboard.Value.Submit( client, score );

	//}

	//[ConCmd.Server( "noclip" )]
	//static void NoclipCommand()
	//{
	//	if ( ConsoleSystem.Caller == null ) return;

	//	Current?.DoPlayerNoclip( ConsoleSystem.Caller );
	//}

	//public virtual void DoPlayerNoclip( IClient player )
	//{

	//	if ( player.Pawn is JumperPawn basePlayer )
	//	{
	//		if ( basePlayer.DevController is NoclipController )
	//		{
	//			Log.Info( "Noclip Mode Off" );
	//			basePlayer.DevController = null;
	//		}
	//		else
	//		{
	//			Log.Info( "Noclip Mode On" );
	//			basePlayer.DevController = new NoclipController();
	//		}
	//	}
	//}
}
