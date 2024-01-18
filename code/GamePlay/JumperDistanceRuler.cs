using Sandbox;

public sealed class JumperDistanceRuler : Component
{
	[Property] public GameObject StartObject { get; set; }
	[Property] public GameObject EndObject { get; set; }
	[Sync] public RealTimeSince SessionTimer { get; set; } = 0f;
	[Sync] public float Distance { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( StartObject is null || EndObject is null )
			return;

		var start = StartObject.Transform.LocalPosition.z;
		var end = EndObject.Transform.LocalPosition.z;

		var distanceBetween = MathF.Abs( start - end );

		Distance = distanceBetween;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.LineThickness = 12;
		Gizmo.Draw.Line( StartObject.Transform.LocalPosition, EndObject.Transform.LocalPosition );
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.Text( $"{Distance}", new Transform( StartObject.Transform.LocalPosition + (EndObject.Transform.LocalPosition - StartObject.Transform.LocalPosition) / 2 ),"poppins",48 );
	}
}
