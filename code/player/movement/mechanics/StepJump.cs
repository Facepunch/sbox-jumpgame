
using Sandbox;

namespace JumpingSausage.Movement
{
	class StepJump : BaseMoveMechanic
	{

		public float JumpPower => 240f;

		public override bool TakesOverControl => true;

		private Vector3 jumpPos;
		private float moveLen;

		public StepJump( JumpingSausageController controller )
			: base( controller )
		{

		}

		protected override bool TryActivate()
		{
			if ( !InputActions.Walk.Down() ) return false;

			var wall = GetWallInfo( ctrl.Velocity.WithZ( 0 ).Normal );
			if ( wall == null ) return false;

			var wallr = wall.Value;
			if ( wallr.Height < 20 || wallr.Height > 50 ) return false;
			if ( wallr.Distance > 35 ) return false;

			var trStart = ctrl.Position + Vector3.Up * (wallr.Height + 20);
			trStart += ctrl.Velocity.WithZ( 0 ).Normal * (wallr.Distance + ctrl.BodyGirth);
			var trEnd = trStart + Vector3.Down * 50;
			var tr = Trace.Ray( trStart, trEnd ).Ignore( ctrl.Pawn ).Run();

			if ( !tr.Hit || tr.StartedSolid ) return false;

			jumpPos = tr.EndPosition + Vector3.Up * 10;
			moveLen = (jumpPos - ctrl.Position).Length;

			new FallCameraModifier( 300 );

			return true;
		}

		public override void Simulate()
		{
			if ( TimeSinceActivate > 30f )
			{
				IsActive = false;
				return;
			}

			if ( moveLen > 0 )
			{
				var spd = ctrl.Velocity.WithZ( 0 ).Length * 1.5f;
				var dir = (jumpPos - ctrl.Position).Normal;
				var move = dir * spd * Time.Delta;
				ctrl.Position += move;
				moveLen -= move.Length;

				if ( moveLen > 0 ) return;
			}

			var jumpVec = Vector3.Up * JumpPower;

			ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
			ctrl.Velocity += jumpVec;
			ctrl.Position += ctrl.Velocity * Time.Delta;

			ctrl.GetMechanic<Unstucker>()?.Simulate();

			new FallCameraModifier( -500 );

			IsActive = false;
		}

	}
}
