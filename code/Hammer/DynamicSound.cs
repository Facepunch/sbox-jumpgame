using Sandbox;

[GameResource( "Dynamic Sound", "dyns", "dynamic_sound", Icon = "speaker_group", IconBgColor = "#fcba03", IconFgColor = "#363324" )]
public class DynamicSoundResource : GameResource
{
	[Property, ResourceType( "sound" )]
	[Title( "Background Song" )]
	[Category( "Background" )]
	public string BackgroundSong { get; set; }

	[Property, ResourceType( "sound" )]
	[Title( "Flying Song" )]
	[Category( "Flying" )]
	public IList<string> FlyingSong { get; set; }

	[Property, ResourceType( "sound" )]
	[Title( "Terminal Song" )]
	[Category( "Terminal" )]
	public IList<string> TerminalSong { get; set; }

	[Property, ResourceType( "sound" )]
	[Title( "Interactable Loop Song" )]
	[Category( "Interactable" )]
	public string InteractableLoopSong { get; set; }

	[Property, ResourceType( "sound" )]
	[Title( "Interactable End Song" )]
	[Category( "Interactable" )]
	public string InteractableEndSong { get; set; }
}

[Library( "snd_dynamicsound" )]
[Display( Name = "Dynamic Sound", GroupName = "Jumper", Description = "Dynamic Sound." ), Category( "Sound" ), Icon( "speaker_group" )]
[EditorModel( "models/editor/speaker/speaker.vmdl", "#00ffbf" )]
[HammerEntity]
public partial class DynamicSound : Entity
{

	[Property, ResourceType( "dyns" )]
	public string AssetPath { get; set; }

	[Property]
	public bool IsPlaying { get; set; }

	[Net]
	protected DynamicSoundResource Resource { get; set; }

	public Sound BackgroundSound { get; set; }

	[Net]
	public IList<Sound> FlyingSound { get; set; }

	[Net]
	public IList<string> TerminalSong { get; set; }

	public string InteractableLoopSong { get; set; }

	public string InteractableEndSong { get; set; }

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		Resource = ResourceLibrary.Get<DynamicSoundResource>( AssetPath );

		TerminalSong = Resource.TerminalSong;

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		BackgroundSound = PlaySound( Resource.BackgroundSong ).SetVolume( 1 );
	}

	//Single check for player is in the air.
	bool IsFlying;

	[Event.Tick.Client]
	public void SoundTick()
	{

		if ( Local.Pawn is not Player player )
			return;

		if ( FlyingSound.Count <= 0 )
		{
			for ( int i = 0; i < Resource.FlyingSong.Count; i++ )
			{
				FlyingSound.Add( Sound.FromEntity( Resource.FlyingSong[i], player ).SetVolume( 0 ) );
			}
		}

		if ( player.Velocity.Length > 10 )
		{
			if ( player.Controller.GroundEntity == null )
			{
				if ( !IsFlying )
				{
					StartFly();
				}
			}
		}

		if ( player.Controller.GroundEntity != null )
		{
			StartGrounded();
		}

		if ( !IsPlaying )
		{
			MuteAllSound();
		}
	}

	public void MuteAllSound()
	{
		BackgroundSound.SetVolume( 0 );
		FlyingSound[lastflySong].SetVolume( 0 );
		IsFlying = false;
	}

	public void StartGrounded()
	{
		if ( !IsPlaying ) return;

		FlyingSound[lastflySong].SetVolume( 0 );
		IsFlying = false;
		BackgroundSound.SetVolume( MathX.LerpTo( 1f, 0.25f, Time.Delta ) );
	}

	public void StartFly()
	{
		if ( !IsPlaying ) return;

		IsFlying = true;
		GetRandomFlySong();
		FlyingSound[lastflySong].SetVolume( 2 );
		BackgroundSound.SetVolume( MathX.LerpTo( .25f, 1f, Time.Delta ) );

	}

	private int lastflySong;
	private string GetRandomFlySong()
	{
		var idx = Rand.Int( 0, Resource.FlyingSong.Count - 1 );
		while ( idx == lastflySong )
			idx = Rand.Int( 0, Resource.FlyingSong.Count - 1 );

		lastflySong = idx;
		return string.Format( Resource.FlyingSong[idx] );
	}

	[ClientRpc]
	public void OnPlaySound()
	{
		IsPlaying = true;
	}


	[ClientRpc]
	public void OnStopSound()
	{
		IsPlaying = false;
	}

	[Input]
	public void EnableSound()
	{
		OnPlaySound();
	}

	[Input]
	public void DisableSound()
	{
		OnStopSound();
	}

}
[Library( "snd_dynamicsoundlocal" )]
[Display( Name = "Dynamic Sound Local", GroupName = "Jumper", Description = "Dynamic Sound Local." ), Category( "Sound" ), Icon( "speaker" )]
[EditorModel( "models/editor/speaker/speakerlocal.vmdl", "#ff4040" )]
[HammerEntity]
public partial class DynamicSoundlocal : Entity
{
	[Net, Property, FGDType( "target_destination" )] public string TargetDynamicSound { get; set; } = "";
	[Net, Property] public int SoundSelection { get; set; }

	[Net, Property]
	[Title( "Use Velocity for Volume" )]
	public bool VelocityVolume { get; set; }

	[Net, Property]
	[Title( "StartOn" )]
	public bool StartOn { get; set; }

	private DynamicSound DynamicSound;

	public Sound LocalSound { get; set; }

	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		base.Spawn();
	}
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		DynamicSound ??= FindByName( TargetDynamicSound ) as DynamicSound;

		StartPlayingSound();
	}

	[Event.Tick.Client]
	public void Tick()
	{
		if ( VelocityVolume )
		{
			LocalSound.SetVolume( 1 * (Velocity.Length / 1000) );
		}

	}

	[ClientRpc]
	public void StartPlayingSound()
	{
		if ( StartOn )
		{
			LocalSound = Sound.FromEntity( DynamicSound.TerminalSong[SoundSelection], this ).SetVolume( 1 );
		}
		else
		{
			LocalSound = Sound.FromEntity( DynamicSound.TerminalSong[SoundSelection], this ).SetVolume( 0 );
		}
	}

	[ClientRpc]
	public void OnPlaySound()
	{
		LocalSound.SetVolume( 1 );
	}


	[ClientRpc]
	public void OnStopSound()
	{
		LocalSound.SetVolume( 0 );
	}

	[Input]
	public void StartSound()
	{
		OnPlaySound();
	}

	[Input]
	public void StopSound()
	{
		OnStopSound();
	}
}

[Library( "snd_dynamicsoundboxlocal" )]
[Display( Name = "Dynamic Sound Box Local", GroupName = "Jumper", Description = "Dynamic Sound  Local." ), Category( "Sound" ), Icon( "surround_sound" )]
[EditorModel( "models/editor/speaker/speaker_box.vmdl", "#e0a604" )]
[BoundsHelper( "mins", "maxs", true, false )]
[HammerEntity]
public partial class DynamicSoundBoxlocal : Entity
{
	[Net, Property, FGDType( "target_destination" )] public string TargetDynamicSound { get; set; } = "";
	[Net, Property] public int SoundSelection { get; set; }


	[Property( "mins", Title = "Box Mins" ), Category( "Box Size" )]
	[Net]
	[DefaultValue( "-32 -32 -32" )]
	public Vector3 Mins { get; set; } = new Vector3( -32, -32, -32 );

	[Property( "maxs", Title = "Box Maxs" ), Category( "Box Size" )]
	[Net]
	[DefaultValue( "32 32 32" )]
	public Vector3 Maxs { get; set; } = new Vector3( 32, 32, 32 );


	private DynamicSound DynamicSound;

	[Net]
	public BBox Inner { get; private set; }
	public Sound LocalSound { get; set; }
	public Vector3 SndPos { get; private set; }
	public override void Spawn()
	{
		Transmit = TransmitType.Always;

		Inner = new BBox( Position + Mins, Position + Maxs );

		base.Spawn();
	}
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		DynamicSound ??= FindByName( TargetDynamicSound ) as DynamicSound;

		StartPlayingSound();
	}
	private void ShortestDistanceToSurface( Vector3 position )
	{
		var innerclosetsPoint = Inner.ClosestPoint( position );

		SndPos = innerclosetsPoint;
	}

	[Event.Tick.Client]
	public void Tick()
	{
		LocalSound.SetPosition( SndPos );

		var pos = CurrentView.Position;
		if ( Local.Pawn.IsValid() )
		{
			pos = Local.Pawn.EyePosition;

		}
		ShortestDistanceToSurface( pos );
	}

	[ClientRpc]
	public void StartPlayingSound()
	{
		LocalSound = Sound.FromWorld( DynamicSound.TerminalSong[SoundSelection], Position ).SetVolume( 0 );
	}

	[ClientRpc]
	public void OnPlaySound()
	{
		LocalSound.SetVolume( 1 );
	}


	[ClientRpc]
	public void OnStopSound()
	{
		LocalSound.SetVolume( 0 );
	}

	[Input]
	public void StartSound()
	{
		OnPlaySound();
	}

	[Input]
	public void StopSound()
	{
		OnStopSound();
	}

}
