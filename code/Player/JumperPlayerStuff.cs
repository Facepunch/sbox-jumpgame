using Sandbox;

public sealed class JumperPlayerStuff : Component, Component.INetworkSpawn
{
	public string PlayerName { get; set; } = "bakscratch";
	public ulong SteamId { get; set; }
	public string PlayerAvatar { get; set; } = "https://files.facepunch.com/louie/1b3011b1/a_1dbd09c2dbb63a7794ab61c80ce205cf.gif";
	public float Height { get; set; }
	public float MaxHeight { get; set; }
	public int TotalJumps { get; set; }
	public int TotalFalls { get; set; }
	public float TimePlayed { get; set; }
	public int Completions { get; set; }
	public bool ReachedEnd { get; set; }
	public float BestHeight { get; set; }
	public string rndColor { get; set; } = "#eb4034";

	public Vector3 Position { get; set; }
	public Angles Angles { get; set; }

	[Property] JumperDistanceRuler DistanceRuler { get; set; }
	[Property] JumperProgress Progress { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		PlayerName = owner.DisplayName;
		PlayerAvatar = owner.SteamId.ToString();
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
		if ( IsProxy ) return;

		if ( DistanceRuler == null || DistanceRuler.StartObject == null )
		{
		//	Log.Warning( "No distance ruler / distance ruler object found." );
			return;
		}

		Height = MathX.CeilToInt( Transform.Position.z - DistanceRuler.StartObject.Transform.Position.z );

		MaxHeight = Math.Max( Height, MaxHeight );
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( PlayerName );
		stream.Write( SteamId );
		stream.Write( PlayerAvatar );
		stream.Write( Height );
		stream.Write( MaxHeight );
	}

	public void Read( ByteStream stream )
	{
		PlayerName = stream.Read<string>();
		SteamId = stream.Read<ulong>();
		PlayerAvatar = stream.Read<string>();
		Height = stream.Read<float>();
		MaxHeight = stream.Read<float>();
	}

	public void OnActive( Connection channel )
	{

		PlayerName = channel.DisplayName;
		SteamId = channel.SteamId;
		PlayerAvatar = $"avatar:{SteamId}";

		Log.Info( $"JumperPlayer '{PlayerName}, {SteamId}, {PlayerAvatar}' is becoming active, " );
	}
}
