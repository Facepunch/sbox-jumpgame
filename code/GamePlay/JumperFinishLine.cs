using Sandbox;

[Title( "Jumper - Finish" )]
[Category( "Physics" )]
[Icon( "directions_walk", "red", "white" )]
[EditorHandle( "materials/gizmo/charactercontroller.png" )]
public sealed class JumperFinishLine : Component, Component.ITriggerListener
{
	[Property] public GameObject TopPosition { get; set; }
	
	List<GameObject> Players = new();
	
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "player" ) )
		{
			var ply = other.GameObject.Parent;
			var plyComp = ply.Components.Get<JumperEndUI>( FindMode.EnabledInSelfAndChildren );
			plyComp.Open = true;
			Players.Add( ply );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			var ply = other.GameObject.Parent;
			var plyComp = ply.Components.Get<JumperEndUI>(FindMode.EnabledInSelfAndChildren);
			plyComp.Open = false;		
			Players.Remove( ply );
		}

	}
}
