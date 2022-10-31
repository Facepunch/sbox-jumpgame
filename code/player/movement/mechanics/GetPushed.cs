
using Sandbox;
using System.Linq;

namespace JumpingSausage.Movement
{
	class GetPushed : BaseMoveMechanic
	{

		public override bool AlwaysSimulate => true;

		public GetPushed( JumpingSausageController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PreSimulate();

			var bbox = new BBox( ctrl.Position + ctrl.Mins, ctrl.Position + ctrl.Maxs );
			var ents = Entity.FindInBox( bbox );

			foreach( var ent in ents )
			{
				if ( !CanBePushedBy( ent ) ) continue;

				var mover = new MoveHelper( ctrl.Position, ent.Velocity );
				mover.Trace = mover.Trace.Ignore( ctrl.Pawn );
				mover.TryMove( Time.Delta );
				ctrl.Position = mover.Position;
			}
		}

		private bool CanBePushedBy( Entity ent )
		{
			if ( ent is PlatformEntity ) return true;

			return false;
		}

	}
}
