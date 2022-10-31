
using Sandbox;

namespace JumpingSausage.Movement
{
	class Unstucker : BaseMoveMechanic
	{

		public override bool AlwaysSimulate => true;

		private int _stuckTries = 0;

		public Unstucker( JumpingSausageController ctrl )
			: base( ctrl )
		{
		}

		public override void Simulate()
		{
			var result = ctrl.TraceBBox( ctrl.Position, ctrl.Position );

			// Not stuck, we cool
			if ( !result.StartedSolid )
			{

				_stuckTries = 0;
				return;
			}

			if ( BasePlayerController.Debug )
			{
				DebugOverlay.Text( $"[stuck in {result.Entity}]", ctrl.Position, Color.Red );
				Box( result.Entity, Color.Red );
			}

			if ( Host.IsClient )
				return;

			int AttemptsPerTick = 20;

			for ( int i = 0; i < AttemptsPerTick; i++ )
			{
				var pos = ctrl.Position + Vector3.Random.Normal * (((float)_stuckTries) / 2.0f);

				// First try the up direction for moving platforms
				if ( i == 0 )
				{
					pos = ctrl.Position + Vector3.Up * 5;
				}

				result = ctrl.TraceBBox( pos, pos );

				if ( !result.StartedSolid )
				{
					if ( BasePlayerController.Debug )
					{
						DebugOverlay.Text( $"unstuck after {_stuckTries} tries ({_stuckTries * AttemptsPerTick} tests)", ctrl.Position, Color.Green, 5.0f );
						DebugOverlay.Line( pos, ctrl.Position, Color.Green, 5.0f, false );
					}

					ctrl.Position = pos;
					return;
				}
				else
				{
					if ( BasePlayerController.Debug )
					{
						DebugOverlay.Line( pos, ctrl.Position, Color.Yellow, 0.5f, false );
					}
				}
			}

			_stuckTries++;
		}

		public void Box( Entity ent, Color color, float duration = 0.0f )
		{
			if ( ent is ModelEntity modelEnt )
			{
				var bbox = modelEnt.CollisionBounds;
				DebugOverlay.Box( modelEnt.Position, modelEnt.Rotation, bbox.Mins, bbox.Maxs, color, duration );
			}
			else
			{
				DebugOverlay.Box( ent.Position, ent.Rotation, -1, 1, color, duration );
			}
		}

	}
}
