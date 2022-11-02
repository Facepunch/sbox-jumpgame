
[Library( "js_ground", Description = "Ground Level" )]
[EditorSprite( "materials/editor/assault_rally.vmat" )]
[Display( Name = "Ground Level", GroupName = "Jumper", Description = "Ground Level" ), Category( "Gameplay" ), Icon( "grass" )]
[HammerEntity]
[BoundsHelper( "mins", "maxs", false, true )]
partial class MusicBoxTweaker : ModelEntity
{
	[Property( "mins", Title = "Mins" )]
	[Net]
	[DefaultValue( "-64 -64 -64" )]
	public Vector3 Mins { get; set; } = new Vector3( -64, -64, -64 );

	[Property( "maxs", Title = "Maxs" )]
	[Net]
	[DefaultValue( "64 64 64" )]
	public Vector3 Maxs { get; set; } = new Vector3( 64, 64, 64 );

	[Net]
	public BBox Outer { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Outer = new BBox( Position + Mins, Position + Maxs );
	}
}
