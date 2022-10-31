
using Sandbox;

namespace JumpingSausage.Movement
{
	partial class BaseMoveMechanic : BaseNetworkable
	{

		public virtual string HudName { get; protected set; }
		public virtual string HudDescription { get; protected set; }

		public bool IsActive { get; protected set; }
		public virtual bool AlwaysSimulate { get; private set; }
		public virtual bool TakesOverControl { get; }
		public TimeSince TimeSinceActivate { get; private set; }
		public virtual float EyePosMultiplier => 1f;

		protected JumpingSausageController ctrl;

		public BaseMoveMechanic( JumpingSausageController controller )
		{
			ctrl = controller;
		}

		public bool Try()
		{
			IsActive = TryActivate();
			if ( IsActive )
			{
				TimeSinceActivate = 0;

				if ( BasePlayerController.Debug )
				{
					Log.Info( "ACTIVATED: " + GetType().Name );
				}
			}

			return IsActive;
		}

		public virtual void PreSimulate() { }
		public virtual void PostSimulate() { }
		public virtual void Simulate() { }
		public virtual void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f ) { }
		public virtual float GetWishSpeed() { return -1f; }
		protected virtual bool TryActivate() { return false; }

		protected WallInfo? GetWallInfo( Vector3 direction )
		{
			var trace = ctrl.TraceBBox( ctrl.Position, ctrl.Position + direction * ctrl.BodyGirth * 2 );
			if ( !trace.Hit ) return null;

			var height = ApproximateWallHeight( ctrl.Position, trace.Normal, 500f, 100f, 32 );

			return new WallInfo()
			{
				Height = height,
				Distance = trace.Distance,
				Normal = trace.Normal,
				Trace = trace
			};
		}

		private static float ApproximateWallHeight( Vector3 startPos, Vector3 wallNormal, float maxHeight, float maxDist, int precision = 16 )
		{
			var step = maxHeight / precision;
			var wallFoudn = false;
			for ( int i = 0; i < precision; i++ )
			{
				startPos.z += step;
				var trace = Trace.Ray( startPos, startPos - wallNormal * maxDist )
					.WorldOnly()
					.Run();

				if ( !trace.Hit && !wallFoudn ) continue;
				if ( trace.Hit )
				{
					wallFoudn = true;
					continue;
				}

				return startPos.z;
			}
			return 0f;
		}

	}

	public struct WallInfo
	{
		public float Distance;
		public Vector3 Normal;
		public float Height;
		public TraceResult Trace;
	}

}
