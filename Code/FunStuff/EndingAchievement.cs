using Sandbox.Services;

namespace Sandbox;

public sealed class EndingAchievement : Component, Component.ITriggerListener
{
	void ITriggerListener.OnTriggerEnter( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			if ( !other.Network.IsProxy )
			{
				Achievements.Unlock( "get_to_the_top" );
			}
		}
	}
}
