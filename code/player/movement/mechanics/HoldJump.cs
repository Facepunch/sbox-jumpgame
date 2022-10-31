
using Sandbox;

namespace JumpingSausage.Movement
{
	class HoldJump : BaseMoveMechanic
	{

		public override string HudName => "Hold Jump";
		public override string HudDescription => $"Press {InputActions.Duck.GetButtonOrigin()}+{InputActions.Jump.GetButtonOrigin()} while running";

		public override bool AlwaysSimulate => true;
		public override bool TakesOverControl => false;

		public float JumpPower => 250f;

		public float HeldJumpTime;

		public bool CanJump = true;

		public HoldJump( JumpingSausageController controller )
			: base( controller )
		{

		}

		public override void PreSimulate()
		{
			base.PostSimulate();

			if ( Input.Released( InputButton.Jump ) && !ctrl.GroundEntity.IsValid() )
			{
				CanJump = true;
			}

			if ( !ctrl.GroundEntity.IsValid() )
				return;

			if ( InputActions.Jump.Down() && CanJump )
			{
				HeldJumpTime += .05f;

				ctrl.IsAboutToJump = true;

				ctrl.SetTag( "ducked" );
				Log.Info( HeldJumpTime );
				ctrl.Velocity = 0;

				if ( HeldJumpTime >= 2.5 )
				{
					DoJump();
					CanJump = false;
				}
			}

			if ( Input.Released( InputButton.Jump ) )
			{
				CanJump = true;
				if ( HeldJumpTime > 0 )
				{
					DoJump();
					Log.Info( "Jump" );
					ctrl.IsAboutToJump = false;

				}
			}
		}

		private void DoJump()
		{
			var flMul = JumpPower;
			var startz = ctrl.Velocity.z;
			var jumpPower = startz + flMul * HeldJumpTime;


			ctrl.Velocity = ctrl.Rotation.Forward * HeldJumpTime * flMul / 2;
			ctrl.Velocity = ctrl.Velocity.WithZ( jumpPower );
			//ctrl.Velocity = ctrl.Velocity.WithZ( jumpPower );
			ctrl.AddEvent( "jump" );

			new FallCameraModifier( jumpPower );

			ctrl.ClearGroundEntity();
			ctrl.IsAboutToJump = false;
			HeldJumpTime = 0;
		}

		private void LongJumpEffect()
		{
			ctrl.AddEvent( "jump" );

			if ( !ctrl.Pawn.IsServer ) return;
			using var _ = Prediction.Off();

			var particle = Particles.Create( "particles/gameplay/player/longjumptrail/longjumptrail.vpcf", ctrl.Pawn );
			Sound.FromWorld( "player.ljump", ctrl.Pawn.Position );
		}

	}
}
