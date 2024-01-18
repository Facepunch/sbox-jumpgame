using Sandbox;

public sealed class JumperPlayerStuff : Component, Component.INetworkListener
{
	[Sync] public string PlayerName { get; set; }
	[Sync] public ulong SteamId { get; set; }
	[Sync] public string PlayerAvatar { get; set; }
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

	void Component.INetworkListener.OnConnected( Sandbox.Connection channel )
	{
		PlayerName = channel.DisplayName.ToString();
		PlayerAvatar = channel.SteamId.ToString();
		SteamId = channel.SteamId;
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		PlayerName = channel.DisplayName.ToString();
		PlayerAvatar = channel.SteamId.ToString();
		SteamId = channel.SteamId;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		rndColor = $"{Color.Random.Hex}";
		DistanceRuler = Scene.GetAllComponents<JumperDistanceRuler>().FirstOrDefault();
	}

	public void SaveStats()
	{
		if ( IsProxy ) return;

		//Log.Info( "Saving stats" );

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
		Progress.Current.Position = GameObject.Transform.Position;
		var plycontroller = GameObject.Components.Get<JumperPlayerController>(FindMode.EnabledInSelf);
		Progress.Current.Angles = plycontroller.TargetAngles;
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
		Progress.Current.Position = GameObject.Transform.Position;
		Progress.Current.Angles = GameObject.Transform.Rotation.Angles();
		Progress.Save();
	}

	protected override void OnFixedUpdate()
	{
		Height = MathX.CeilToInt( Transform.Position.z - DistanceRuler.StartObject.Transform.Position.z );

		MaxHeight = Math.Max( Height, MaxHeight );
	}
}
