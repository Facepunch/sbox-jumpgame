
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using JumpingSausage.Movement;

namespace JumpingSausage
{
	partial class JumpingSausageDeadPawn : Sandbox.Player
	{

		[Net]
		public string CurrentArea { get; set; } = "Dead";

		[Net]
		public Color PlayerColor { get; set; }

		public JumpingSausageDeadPawn() { }

		public JumpingSausageDeadPawn( Client cl )
		{

		}

		public override void Spawn()
		{

			SetModel( "models/gameplay/spectator_head/spectator_head.vmdl" );

			Controller = new FlyingController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new JumpingSausageSpectateCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			PlayerColor = Color.Random;

			RenderColor = PlayerColor;
			RenderColor = RenderColor.WithAlpha( .5f );
			

			base.Spawn();

		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			SimulateActiveChild( cl, ActiveChild );

		}
	}
}
