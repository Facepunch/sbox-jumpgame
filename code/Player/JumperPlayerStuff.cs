using Sandbox;

public sealed class JumperPlayerStuff : Component, Component.INetworkListener
{
	[Sync] public float Height { get; set; } 
	[Sync] public float MaxHeight { get; set; }
	public int TotalJumps { get; set; }
	public int TotalFalls { get; set; }
	public float TimePlayed { get; set; }
	public int Completions { get; set; }
	public bool ReachedEnd { get; set; }
	public float BestHeight { get; set; }
	public string rndColor { get; set; } = "#eb4034";

	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }

	JumperDistanceRuler DistanceRuler { get; set; }
	[Property] JumperProgress Progress { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		rndColor = $"{Color.Random.Hex}";
		DistanceRuler = Scene.GetAllComponents<JumperDistanceRuler>().FirstOrDefault();

		/*
		PlayerAvatar = GameObject.Network.OwnerConnection.SteamId.ToString();
		SteamId = GameObject.Network.OwnerConnection.SteamId;
		*/
	}

	protected override void OnAwake()
	{
		base.OnAwake();

	}

	public void SaveStats()
	{
		if ( IsProxy ) return;

		if ( Progress == null || Progress.Current == null )
		{
			Log.Warning( "No progress found on JumperPlayerStuff" );
			return;
		}

		Progress.Current.BestHeight = MaxHeight;
		Progress.Current.TotalJumps = TotalJumps;
		Progress.Current.TotalFalls = TotalFalls;
		Progress.Current.NumberCompletions = Completions;
		Progress.Current.TimePlayed = TimePlayed;
		Progress.Current.Position = GameObject.WorldPosition;
		var plycontroller = GameObject.Components.Get<PlayerController>(FindMode.EnabledInSelf);
		Progress.Current.Angles = plycontroller.Body.WorldRotation.Angles();
		Progress.Save();
	}

	public void Restart()
	{
		TotalJumps = 0;
		TotalFalls = 0;
		TimePlayed = 0;
		ReachedEnd = false;
		MaxHeight = 0;
		Progress.Current.BestHeight = 0;
		Progress.Current.TotalJumps = 0;
		Progress.Current.TotalFalls = 0;
		Progress.Current.TimePlayed = 0;
		Progress.Current.Position = GameObject.WorldPosition;
		Progress.Current.Angles = GameObject.WorldRotation.Angles();
		Progress.Save();
	}

	protected override void OnFixedUpdate()
	{
		Height = MathX.CeilToInt( WorldPosition.z - DistanceRuler.StartObject.WorldPosition.z );

		MaxHeight = Math.Max( Height, MaxHeight );
	}
}
