
using Sandbox;

namespace JumpingSausage.Movement
{
	class PlayerBounce : BaseMoveMechanic
	{

		public override string HudName => "Hold Jump";
		public override string HudDescription => $"Press {InputActions.Duck.GetButtonOrigin()}+{InputActions.Jump.GetButtonOrigin()} while running";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;
		
		public WallInfo Wall { get; private set; }

		private Vector3 prevWallRunStart;

		public PlayerBounce( JumpingSausageController controller )
			: base( controller )
		{

		}

		protected override bool TryActivate()
		{
			if ( ctrl.GroundEntity != null ) return false;
			if ( ctrl.Velocity.z > 100 ) return false;
			if ( ctrl.Velocity.z < -150 ) return false;

			var wall = FindRunnableWall();
			if ( wall == null ) return false;

			var startPos = wall.Value.Trace.EndPosition;
			var dist = prevWallRunStart.WithZ( 0 ).Distance( startPos.WithZ( 0 ) );

			// check x dist is a certain amount to avoid wallrunning straight up multiple times

			Wall = wall.Value;
			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			prevWallRunStart = wall.Value.Trace.EndPosition;

			return true;
		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( ctrl.GroundEntity != null ) return;
			var wall = FindRunnableWall();
			if ( wall == null ) return;

			var bounce = Vector3.Reflect( ctrl.Velocity, wall.Value.Normal );

			var trStart = ctrl.Position + Wall.Normal;
			var trEnd = ctrl.Position - Wall.Normal * ctrl.BodyGirth * 2;
			var tr = ctrl.TraceBBox( trStart, trEnd );

			Sound.FromEntity( "jump_pad", ctrl.Pawn );

			ctrl.Velocity = bounce;
			ctrl.Rotation = Rotation.LookAt( wall.Value.Trace.Normal * 10.0f, Vector3.Up );
			DebugOverlay.Line( ctrl.Pawn.Position,bounce );
		}
		private WallInfo? FindRunnableWall()
		{
			var wall = GetWallInfo( ctrl.Rotation.Forward );

			if ( wall == null ) return null;
			if ( wall.Value.Distance > ctrl.BodyGirth ) return null;
			if ( !wall.Value.Normal.z.AlmostEqual( 0, .1f ) ) return null;

			return wall;
		}
	}
}
