using Sandbox.UI.Construct;

public class VoiceChatEntry : Panel
{
	public Friend Friend;
	readonly Image Avatar;

	private float VoiceLevel = 0.0f;
	private float TargetVoiceLevel = 0;

	RealTimeSince timeSincePlayed;

	public VoiceChatEntry( Panel parent, long steamId )
	{
		Parent = parent;
		
		Friend = new Friend( steamId );

		Avatar = Add.Image( "", "avatar" );
		Avatar.SetTexture( $"avatar:{steamId}" );
	}

	public void Update( float level )
	{
		timeSincePlayed = 0;
		TargetVoiceLevel = level;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDeleting )
			return;

		var SpeakTimeout = 2.0f;
		var timeoutInv = 1 - (timeSincePlayed / SpeakTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		//VoiceLevel = VoiceLevel.LerpTo( TargetVoiceLevel, Time.Delta * 40.0f );
		//var tr = new PanelTransform();
		//tr.AddScale( 1.0f.LerpTo( 1.2f, VoiceLevel ) );
		//Style.Transform = tr;
		Style.Dirty();
	}
}
