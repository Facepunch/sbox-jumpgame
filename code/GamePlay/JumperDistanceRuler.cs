using Sandbox;

public sealed class JumperDistanceRuler : Component
{
	[Property] public GameObject StartObject { get; set; }
	[Property] public GameObject EndObject { get; set; }
	public RealTimeSince SessionTimer { get; set; } = 0f;
	public float Distance { get; private set; }


	protected override void OnStart()
	{
		base.OnStart();

		if ( StartObject is null || EndObject is null )
			return;

		var distanceBetween = Vector3.DistanceBetween( StartObject.Transform.Position.z, EndObject.Transform.Position.z );

		Distance = distanceBetween;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var distanceBetween = Vector3.DistanceBetween( StartObject.Transform.Position.z, EndObject.Transform.Position.z );

		Distance = distanceBetween;

		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.LineThickness = 12;
		Gizmo.Draw.Line( StartObject.Transform.LocalPosition, EndObject.Transform.LocalPosition );
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.Text( $"{Distance}m", new Transform( StartObject.Transform.LocalPosition + (EndObject.Transform.LocalPosition - StartObject.Transform.LocalPosition) / 2 ),"poppins",48 );
	}

	protected override void OnUpdate()
	{

	}
}
