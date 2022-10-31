
using Sandbox;

namespace JumpingSausage.Movement
{
	class MoveDebug : BaseMoveMechanic
	{

		public override bool AlwaysSimulate => true;

		public MoveDebug( JumpingSausageController ctrl )
			: base( ctrl )
		{

		}

		public override void PostSimulate()
		{
			if ( BasePlayerController.Debug )
			{
				var boxColor = Host.IsServer ? Color.Red : Color.Green;
				DebugOverlay.Box( ctrl.Position + ctrl.TraceOffset, ctrl.Mins, ctrl.Maxs, boxColor );
				DebugOverlay.Box( ctrl.Position, ctrl.Mins, ctrl.Maxs, boxColor );

				var lineOffset = Host.IsServer ? 10 : 0;
				var printTime = Host.IsServer ? 0f : 0.04f;
				// todo: print this shit so it doesn't flicker
				DebugOverlay.ScreenText( $"        Position: {ctrl.Position}", lineOffset + 0, printTime );
				DebugOverlay.ScreenText( $"        Velocity: {ctrl.Velocity}", lineOffset + 1, printTime );
				DebugOverlay.ScreenText( $"           Speed: {ctrl.Velocity.Length}", lineOffset + 2, printTime );
				DebugOverlay.ScreenText( $"    BaseVelocity: {ctrl.BaseVelocity}", lineOffset + 3, printTime );
				DebugOverlay.ScreenText( $"    GroundEntity: {ctrl.GroundEntity} [{ctrl.GroundEntity?.Velocity}]", lineOffset + 4, printTime );
				DebugOverlay.ScreenText( $"    WishVelocity: {ctrl.WishVelocity}", lineOffset + 5, printTime );
			}
		}

	}
}
