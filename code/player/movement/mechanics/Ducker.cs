
using Sandbox;

namespace JumpingSausage.Movement
{
	class Ducker : BaseMoveMechanic
	{

		private Vector3 originalMins;
		private Vector3 originalMaxs;

		public override float EyePosMultiplier => .5f;
		public float DuckSpeed => 110f;
		public float MaxDuckSpeed => 140f;

		public Ducker( JumpingSausageController ctrl )
			: base( ctrl )
		{

		}

		protected override bool TryActivate()
		{
			if ( !InputActions.Duck.Down() ) return false;
			if ( ctrl.GroundEntity == null ) return false;
			//let slide activate if we too fast
			if ( ctrl.GroundEntity != null && ctrl.Velocity.WithZ( 0 ).Length > MaxDuckSpeed ) return false;

			new FallCameraModifier( 100 );

			return true;
		}

		public override void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f )
		{
			originalMins = mins;
			originalMaxs = maxs;

			maxs = maxs.WithZ( 36 * scale );
		}

		public override void Simulate()
		{
			if ( ctrl.GroundEntity != null && ctrl.Velocity.WithZ( 0 ).Length > MaxDuckSpeed )
			{
				IsActive = false;
				return;
			}

			ctrl.SetTag( "ducked" );

			if ( InputActions.Duck.Down() ) return;

			var pm = ctrl.TraceBBox( ctrl.Position, ctrl.Position, originalMins, originalMaxs );
			if ( pm.StartedSolid ) return;

			IsActive = false;
		}

		public override float GetWishSpeed()
		{
			return DuckSpeed;
		}

	}
}
