public sealed partial class GameManager : GameObjectSystem<GameManager>, Component.INetworkListener, ISceneStartup
{
	public GameManager( Scene scene ) : base( scene )
	{
	}

	void ISceneStartup.OnHostInitialize()
	{
		if ( !Networking.IsActive )
		{
			Networking.CreateLobby( new Sandbox.Network.LobbyConfig() { MaxPlayers = 32 } );
		}
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		SpawnPlayer( channel );
	}

	public void SpawnPlayer( Connection owner )
	{
		// Find a spawn location for this player
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var playerGo = GameObject.Clone( "/prefabs/player/jumperplayer.prefab", new CloneConfig { Name = owner.DisplayName, StartEnabled = false, Transform = startLocation } );
		playerGo.NetworkSpawn( owner );
	}

	/// <summary>
	/// In the editor, spawn the player where they're looking
	/// </summary>
	public static Transform EditorSpawnLocation { get; set; }

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();

		if ( spawnPoints.Length == 0 )
		{
			if ( Application.IsEditor && !EditorSpawnLocation.Position.IsNearlyZero() )
			{
				return EditorSpawnLocation;
			}

			return Transform.Zero;
		}

		return Random.Shared.FromArray( spawnPoints ).Transform.World;
	}
}
