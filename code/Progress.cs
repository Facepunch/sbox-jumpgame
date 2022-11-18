
internal class Progress
{

	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public float TimePlayed { get; set; }
	public float BestHeight { get; set; }
	public int TotalJumps { get; set; }//never reset this
	public int TotalFalls { get; set; }//never reset this
	public int NumberCompletions { get; set; }//never reset this

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
