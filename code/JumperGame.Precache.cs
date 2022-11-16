public partial class JumperGame
{
	static void AddToPrecache()
	{
		// Particles don't have a smart way to precache like models and materials do
		Precache.Add( "particles/player/falling/jumper.falling.vpcf" );
		Precache.Add( "particles/player/impact/jumper.impact.wall.vpcf" );
		Precache.Add( "particles/player/jump/jumper.jump.vpcf" );
		Precache.Add( "particles/player/land/jumper.land.vpcf" );
	}
}
