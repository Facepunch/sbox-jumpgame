using Sandbox;
using Sandbox.Citizen;
using Sandbox.Internal;

public sealed class JumperNPCLooker : Component, Component.ITriggerListener
{
	[Property]
	public CitizenAnimationHelper animhelp { get; set; }


	[Property]
	GameObject LookTarget { get; set; }

	protected override void OnUpdate()
	{
		if ( LookTarget is null )
			return;
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			animhelp.LookAt = other.GameObject.Parent;
			animhelp.EyesWeight = 1;
			animhelp.HeadWeight = 1;
			animhelp.BodyWeight = 1;
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			animhelp.LookAt = LookTarget;

		}

	}
}
