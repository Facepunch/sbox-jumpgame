
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using JumpingSausage;
using JumpingSausage.Movement;

namespace JumpingSausage
{
	partial class JumpingSausagePawn : Sandbox.Player
	{

		public const float MaxRenderDistance = 128f;
		public const float InvulnerableTimeAfterDamaged = 2f;
		
		private Particles FakeShadow;
		private DamageInfo lastDamage;

		public bool IgnoreFallDamage = false;

		public float Height { get; set; }
		

		[Net]
		public TimeSince TimeSinceDamaged { get; set; }

		[Net]
		public PropCarriable HeldBody { get; set; }

		public JumpingSausagePawn()
		{

		}

		private ClothingContainer Clothing;
		[Net] public string ClothingAsString { get; set; }

		public JumpingSausagePawn( Client cl )
		{
			Clothing = new ClothingContainer();
			Clothing.LoadFromClient( cl );
			ClothingAsString = cl.GetClientData( "avatar", "" );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new JumpingSausageController();
			Animator = new JumpingSausageLookAnimator();
			CameraMode = new JumpingSausageOrbitCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Clothing.DressEntity( this );
			//FakeShadow = Particles.Create( "particles/gameplay/fake_shadow/fake_shadow.vpcf", this );

			base.Respawn();

			Health = 4;

			Tags.Add( "Platplayer" );
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( TimeSinceDamaged < InvulnerableTimeAfterDamaged ) return;

			base.TakeDamage( info );

			TimeSinceDamaged = 0;
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			CameraMode = new JumpingSausageRagdollCamera();

			foreach ( var child in Children )
			{
				child.EnableDrawing = false;
			}
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( Input.Pressed( InputButton.Drop ) || Input.Pressed( InputButton.Reload ) )
			{
				Game.Current.DoPlayerSuicide( cl );
			}

		}

		/// <summary>
		/// Called every frame on the client
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );
		}

		[Event.Frame]
		private void UpdateRenderAlpha()
		{
			if ( Local.Pawn == this ) return;
			if ( Local.Pawn == null ) return;
			if ( !Local.Pawn.IsValid() ) return;

			var dist = Local.Pawn.Position.Distance( Position );
			var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
			a = Math.Max( a, .15f );
			a = Easing.EaseOut( a );

			this.RenderColor = this.RenderColor.WithAlpha( a );

			foreach ( var child in this.Children )
			{
				if ( child is not ModelEntity m || !child.IsValid() ) continue;
				m.RenderColor = m.RenderColor.WithAlpha( a );
			}
		}

		public void ApplyForce( Vector3 force )
		{
			//if ( Controller is PlatformerWalkController controller )
			//{
			//	controller.Impulse += force;
			//}
		}
	}
}
