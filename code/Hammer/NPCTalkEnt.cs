
/// <summary>
/// A simple trigger volume that fires once and then removes itself.
/// </summary>
[Library( "js_npctalk" )]
[AutoApplyMaterial( "materials/editor/landmark/landmark.vmat" )]
[Solid]
[Display( Name = "NPCTalk", GroupName = "Jumper", Description = "NPCTalk" ), Category( "Triggers" ), Icon( "landscape" )]
[HammerEntity]
public partial class NPCTalkEnt : TriggerOnce
{

	[Property( "Text", Title = "Land" )]
	public string NPCText { get; set; } = "";

	public override void OnTouchStart( Entity other )
	{
		base.OnTouchStart( other );

		if ( !IsServer ) return;
		if ( other is not JumperPawn pawn ) return;

		NewCheckPoint( To.Single( other ), $"{NPCText}" );
	}

	[ClientRpc]
	public static void NewCheckPoint( string Title )
	{
		NPCTalk.Display( Title );
	}

}
