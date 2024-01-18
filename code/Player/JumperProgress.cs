using Sandbox;

public class JumperProgressData
{
	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }
	public float TimePlayed { get; set; }
	public float BestHeight { get; set; }
	public int TotalJumps { get; set; }//never reset this
	public int TotalFalls { get; set; }//never reset this
	public int NumberCompletions { get; set; }//never reset this
	public bool HasCompleted { get; set; }
}

public sealed class JumperProgress : Component
{
	static string FileName => $"{GameManager.ActiveScene.Title}_progress.json";
	public JumperProgressData Current { get; set; }

	void Fetch()
	{
		Current ??= FileSystem.Data.ReadJson<JumperProgressData>( FileName, null );

		if ( Current != null )
		{
			var player = Components.Get<JumperPlayerStuff>( FindMode.InParent );
			player.MaxHeight = Current.BestHeight;
			player.TotalJumps = Current.TotalJumps;
			player.TotalFalls = Current.TotalFalls;
			player.TimePlayed = Current.TimePlayed;
			player.Completions = Current.NumberCompletions;
			player.Position = Current.Position;
			GameObject.Parent.Transform.Position = Current.Position;
			var plycontroller = GameObject.Components.Get<JumperPlayerController>( FindMode.InAncestors );
			plycontroller.TargetAngles = Current.Angles;
		}
		else
		{
			Current = new();
		}
	}
	protected override void OnStart()
	{
		if(IsProxy)
			return;

		Fetch();
	}

	public void Save()
	{
		// If we didn't load data yet (somehow)
		if ( Current == null )
		{
			Fetch();
		}

		FileSystem.Data.WriteJson( FileName, Current );
	}
}
