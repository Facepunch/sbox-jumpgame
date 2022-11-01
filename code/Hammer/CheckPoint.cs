

using Sandbox;
using SandboxEditor;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


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

	public string LandMarkMessage;
	public string UpperCase;

	public override void Spawn()
	{
		base.Spawn();

		UpperCase = LandMarkName.ToUpper();
		EnableTouchPersists = true;

	}

	[ClientRpc]
	public static void NewCheckPoint( string Title )
	{
		JumperCheckPoint.ShowCheckPoint( Title );
	}

	public override void OnTouchStart( Entity other )
	{
		base.OnTouchStart( other );

		if ( !other.IsServer ) return;
		if ( other is not JumperPawn pl ) return;

		if ( other != null )
		{
			NewCheckPoint( To.Single( other ), $"---ENTERING {UpperCase}---" );

			//Should use this as a place to save the players progress so they can come back to later.
		}
	}
}
