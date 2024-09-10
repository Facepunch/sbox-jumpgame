using Sandbox.Services;

namespace Sandbox;

public sealed class AchievementTrigger : Component, Component.ITriggerListener
{
	[Property] public string Achievement { get; set; } = "";

	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			if ( !other.Network.IsProxy )
			{
				Achievements.Unlock( Achievement );
			}
		}
	}
}
