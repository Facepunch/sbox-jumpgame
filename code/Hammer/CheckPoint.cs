
/// <summary>
/// A simple trigger volume that fires once and then removes itself.
/// </summary>
[Library( "js_landmark" )]
[AutoApplyMaterial( "materials/editor/landmark/landmark.vmat" )]
[Solid]
[Display( Name = "Landmark", GroupName = "Jumper", Description = "Landmark" ), Category( "Triggers" ), Icon( "landscape" )]
[HammerEntity]
public partial class CheckPoint : TriggerMultiple
{

	[Property( "landmarkname", Title = "Land" )]
	public string LandMarkName { get; set; } = "";

	public override void OnTouchStart( Entity other )
	{
		base.OnTouchStart( other );

		if ( !IsServer ) return;
		if ( other is not JumperPawn pawn ) return;

		NewCheckPoint( To.Single( other ), $"---ENTERING {LandMarkName.ToUpper()}---" );
	}

	[ClientRpc]
	public static void NewCheckPoint( string Title )
	{
		CheckpointOverlay.Display( Title );
	}
}
