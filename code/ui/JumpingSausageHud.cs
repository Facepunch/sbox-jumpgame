using Sandbox;
using Sandbox.UI;

namespace JumpingSausage
{
	[UseTemplate]
	public class JumpingSausageHud : RootPanel
	{

		public float Height { get; set; }
		public float MaxHeight { get; set; }

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not JumpingSausagePawn player )
				return;

			if ( Height > MaxHeight )
			{
				MaxHeight = Height;
			}
			else

				Height = player.Height;
		}

	}
}
