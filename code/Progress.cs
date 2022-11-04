
internal class Progress
{

	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public float TimePlayed { get; set; }
	public float BestHeight { get; set; }

	public void Save()
	{
		Cookie.Set( "JumperProgress", this );
	}

	private static Progress current;
	public static Progress Current
	{
		get
		{
			current ??= Cookie.Get<Progress>( "JumperProgress", new() );
			return current;
		}
	}

}
