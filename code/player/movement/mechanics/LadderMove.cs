
using Sandbox;

namespace JumpingSausage.Movement
{
	class LadderMove : BaseMoveMechanic
	{

		public override bool TakesOverControl => true;

		private Vector3 ladderNormal;

		public LadderMove( JumpingSausageController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			const float ladderDistance = 1.0f;
			var start = ctrl.Position;
			var end = start + ctrl.WishVelocity.Normal * ladderDistance;

			var pm = Trace.Ray( start, end )
						.Size( ctrl.Mins, ctrl.Maxs )
						.WithTag( "ladder" )
						.Ignore( ctrl.Pawn )
						.Run();

			if ( pm.Hit )
			{
				ladderNormal = pm.Normal;
				return true;
			}

			return false;
		}

		public override void Simulate()
		{
			if ( InputActions.Jump.Pressed() )
			{
				ctrl.Velocity = ladderNormal * 100.0f;
				IsActive = false;

				return;
			}

			if ( !TryActivate() )
			{
				return;
			}

			var velocity = ctrl.WishVelocity;
			float normalDot = velocity.Dot( ladderNormal );
			var cross = ladderNormal * normalDot;
			ctrl.Velocity = (velocity - cross) + (-normalDot * ladderNormal.Cross( Vector3.Up.Cross( ladderNormal ).Normal ));

			ctrl.Move();
		}

	}
}
