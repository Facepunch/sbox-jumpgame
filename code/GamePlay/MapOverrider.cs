using Sandbox;
using Sandbox.Citizen;

public sealed class MapOverrider : Component
{
	[Property] GameObject FrogPrefab { get; set; }
	protected override void OnEnabled()
	{
		base.OnEnabled();

		{
			var obj = Scene.GetAllComponents<SkinnedModelRenderer>();

			foreach ( var item in obj )
			{
				if ( item.Model.ResourceName == "frog_test_subject_01a" )
					item.Set( "sit", 1 );
					item.Set( "sit_pose", Random.Shared.Float( 3 ) );
					item.Set( "scale_height", Random.Shared.Float( 0.75f, 1.5f ) );
					var ran = Random.Shared.Int( 0, 1 );
					item.MaterialGroup = ran == 1 ? "default" : "Orange";
			}
		}

		{
			var npc = Scene.GetAllObjects( true ).Where( x => x.Name == "js_npc_text" ).ToList();

			foreach ( var item in npc )
			{
				var frogobj = FrogPrefab.Clone();
				frogobj.Transform.Position = item.Transform.Position;
				frogobj.Transform.Rotation = item.Transform.Rotation;
			}
		}
	}

	protected override void OnUpdate()
	{

	}
}
