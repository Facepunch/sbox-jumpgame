using Sandbox;

partial class JumperPawn
{
	[ConCmd.Server]
	public static void SetPosition( Vector3 position, Angles angles )
	{
		var caller = ConsoleSystem.Caller;
		if ( caller?.Pawn is not JumperPawn p ) return;

		p.Position = position + Vector3.Up;
		p.Rotation = Rotation.From( angles );
	}
}
