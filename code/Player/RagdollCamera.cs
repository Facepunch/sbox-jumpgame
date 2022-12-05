
using Sandbox;

public class RagdollCamera : CameraMode
{
	Vector3 FocusPoint;

	public override void Activated()
	{
		base.Activated();

		FocusPoint = CurrentView.Position - GetViewOffset();
		FieldOfView = CurrentView.FieldOfView;
	}

	public override void Update()
	{
		var player = Local.Pawn as Player;
		if ( !player.IsValid() ) return;

		// lerp the focus point
		FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 5.0f );
		var tr = Trace.Ray( FocusPoint + Vector3.Up * 12, FocusPoint + GetViewOffset() )
			.WorldOnly()
			.Ignore( player )
			.Radius( 6 )
			.Run();

		Position = tr.EndPosition;
		Rotation = player.EyeRotation;
		FieldOfView = FieldOfView.LerpTo( 65, Time.Delta * 3.0f );

		Viewer = null;
	}

	public virtual Vector3 GetSpectatePoint()
	{
		if ( Local.Pawn is Player player && player.Corpse.IsValid() )
		{
			return player.Corpse.PhysicsGroup.MassCenter;
		}

		 return Local.Pawn.Position;
	}

	public virtual Vector3 GetViewOffset()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return Vector3.Zero;

		return player.ViewAngles.ToRotation().Forward * (-350 * 1) + Vector3.Up * (20 * 1);
	}
}
