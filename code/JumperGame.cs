
public class JumperGame : Game
{
	public JumperGame()
	{
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

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position += Vector3.Up * 50.0f;
			client.Pawn.Transform = tx;
		}
	}
}
