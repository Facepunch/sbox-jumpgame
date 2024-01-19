using Sandbox;
using Sandbox.Citizen;
using Sandbox.Internal;

public sealed class JumperNPCLooker : Component, Component.ITriggerListener
{
	[Property] public CitizenAnimationHelper animhelp { get; set; }
	[Property] public GameObject LookTarget { get; set; }
	[Property] List<NPCTextGameResource> Resources { get; set; }
	NPCTextGameResource NPCTextGameResource { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( Resources != null && Resources.Count > 0 )
		{
			int randomIndex = new Random().Next( 0, Resources.Count );

			NPCTextGameResource = Resources[randomIndex];
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			animhelp.LookAt = other.GameObject.Parent;
			animhelp.EyesWeight = 1;
			animhelp.HeadWeight = 1;
			animhelp.BodyWeight = 1;
			/*
			var yapper = other.GameObject.Parent.Components.Get<JumperNPCTalker>( FindMode.EnabledInSelfAndChildren );
			yapper.DisplayMessage( GetRandomMessage() );
			yapper.NPCName = NPCTextGameResource.NPCName;
			yapper.Voice = NPCTextGameResource.NPCVoice;
			*/
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			animhelp.LookAt = LookTarget;
		}

	}

	private int lastMessage;
	private string GetRandomMessage()
	{
		var idx = Game.Random.Int( 0, NPCTextGameResource.NPCText.Count - 1 );
		while ( idx == lastMessage )
			idx = Game.Random.Int( 0, NPCTextGameResource.NPCText.Count - 1 );

		lastMessage = idx;
		return string.Format( NPCTextGameResource.NPCText[idx] );
	}
}
