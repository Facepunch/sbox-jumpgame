using Sandbox;
using Sandbox.Internal;

public sealed class ClothingDress : Component
{
	[Property, Title( "Dress" ), Group( "Dress" )]
	SceneModel mod { get; set; }
	 
	protected override void OnEnabled()
	{
		base.OnEnabled();

		var model = Components.Get<SkinnedModelRenderer>();


	}
	protected override void OnUpdate()
	{

	}
}
